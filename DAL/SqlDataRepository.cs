using System.Data;
using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public class SqlDataRepository : IDataRepository
{
    private readonly string _connectionString;

    public SqlDataRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public User? GetUser(string username, string password)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(@"SELECT Id, Username, Password 
                                             FROM Users 
                                             WHERE Username = @username AND Password = @password", connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public bool UsernameExists(string username)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand("SELECT 1 FROM Users WHERE Username = @username", connection);
        command.Parameters.AddWithValue("@username", username);

        connection.Open();
        var exists = command.ExecuteScalar();
        return exists != null && exists != DBNull.Value;
    }

    public User CreateUser(string username, string password)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"INSERT INTO Users (Username, Password) 
              VALUES (@username, @password);
              SELECT CAST(SCOPE_IDENTITY() AS int);", connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        connection.Open();
        var id = (int)(command.ExecuteScalar() ?? 0);
        return new User { Id = id, Username = username, Password = password };
    }

    public List<SchoolClass> GetClassesForTeacher(int teacherId)
    {
        const string sql = @"SELECT Id, Name, Room, Description, TeacherId
                             FROM Classes
                             WHERE TeacherId = @teacherId
                             ORDER BY Name";

        return ReadSchoolClasses(sql, new SqlParameter("@teacherId", teacherId));
    }

    public List<SchoolClass> GetAllClasses()
    {
        const string sql = @"SELECT Id, Name, Room, Description, TeacherId
                             FROM Classes
                             ORDER BY Name";

        return ReadSchoolClasses(sql);
    }

    public SchoolClass AddClass(SchoolClass schoolClass)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"INSERT INTO Classes (Name, Room, Description, TeacherId)
              VALUES (@name, @room, @description, @teacherId);
              SELECT CAST(SCOPE_IDENTITY() AS int);", connection);

        command.Parameters.AddWithValue("@name", schoolClass.Name);
        command.Parameters.AddWithValue("@room", (object?)schoolClass.Room ?? DBNull.Value);
        command.Parameters.AddWithValue("@description", (object?)schoolClass.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@teacherId", schoolClass.TeacherId);

        connection.Open();
        schoolClass.Id = (int)(command.ExecuteScalar() ?? 0);
        return schoolClass;
    }

    public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null)
    {
        var parameters = new List<SqlParameter> { new("@classId", classId) };
        var filter = "Id = @classId";

        if (teacherId.HasValue)
        {
            filter += " AND TeacherId = @teacherId";
            parameters.Add(new SqlParameter("@teacherId", teacherId.Value));
        }

        var classQuery = @$"SELECT Id, Name, Room, Description, TeacherId
                            FROM Classes
                            WHERE {filter}";

        var schoolClass = ReadSchoolClasses(classQuery, parameters.ToArray()).FirstOrDefault();
        if (schoolClass == null)
        {
            return null;
        }

        schoolClass.Enrollments = LoadEnrollmentsForClass(classId);
        return schoolClass;
    }

    public Student? GetStudentByEmail(string email)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"SELECT Id, FirstName, LastName, Email 
              FROM Students 
              WHERE Email = @email", connection);
        command.Parameters.AddWithValue("@email", email);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapStudent(reader) : null;
    }

    public Student AddStudent(Student student)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"INSERT INTO Students (FirstName, LastName, Email)
              VALUES (@first, @last, @email);
              SELECT CAST(SCOPE_IDENTITY() AS int);", connection);

        command.Parameters.AddWithValue("@first", student.FirstName);
        command.Parameters.AddWithValue("@last", student.LastName);
        command.Parameters.AddWithValue("@email", (object?)student.Email ?? DBNull.Value);

        connection.Open();
        student.Id = (int)(command.ExecuteScalar() ?? 0);
        return student;
    }

    public bool EnrollmentExists(int studentId, int classId)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"SELECT 1 FROM Enrollments 
              WHERE StudentId = @studentId AND SchoolClassId = @classId", connection);
        command.Parameters.AddWithValue("@studentId", studentId);
        command.Parameters.AddWithValue("@classId", classId);

        connection.Open();
        var exists = command.ExecuteScalar();
        return exists != null && exists != DBNull.Value;
    }

    public void AddEnrollment(int studentId, int classId)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"INSERT INTO Enrollments (StudentId, SchoolClassId) 
              VALUES (@studentId, @classId)", connection);
        command.Parameters.AddWithValue("@studentId", studentId);
        command.Parameters.AddWithValue("@classId", classId);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId)
    {
        const string sql = @"SELECT e.Id, e.StudentId, e.SchoolClassId,
                                    s.FirstName, s.LastName, s.Email,
                                    c.Id AS ClassId, c.Name, c.Room, c.Description, c.TeacherId
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             INNER JOIN Classes c ON e.SchoolClassId = c.Id
                             WHERE e.Id = @enrollmentId AND c.TeacherId = @teacherId";

        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@enrollmentId", enrollmentId);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ClassEnrollment
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
            SchoolClassId = reader.GetInt32(reader.GetOrdinal("SchoolClassId")),
            Student = MapStudent(reader),
            SchoolClass = MapClass(reader, "ClassId")
        };
    }

    public void RemoveEnrollment(int enrollmentId)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand("DELETE FROM Enrollments WHERE Id = @id", connection);
        command.Parameters.AddWithValue("@id", enrollmentId);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public string? GetClassName(int classId)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand("SELECT Name FROM Classes WHERE Id = @id", connection);
        command.Parameters.AddWithValue("@id", classId);

        connection.Open();
        var result = command.ExecuteScalar();
        return result as string;
    }

    public List<Student> GetStudentsForClass(int classId)
    {
        const string sql = @"SELECT s.Id, s.FirstName, s.LastName, s.Email
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             WHERE e.SchoolClassId = @classId
                             ORDER BY s.LastName, s.FirstName";

        var students = new List<Student>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            students.Add(MapStudent(reader));
        }

        return students;
    }

    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date)
    {
        const string sql = @"SELECT Id, StudentId, SchoolClassId, [Date], IsPresent
                             FROM AttendanceRecords
                             WHERE SchoolClassId = @classId AND [Date] = @date";

        var records = new List<AttendanceRecord>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);
        command.Parameters.AddWithValue("@date", date.Date);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new AttendanceRecord
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                SchoolClassId = reader.GetInt32(reader.GetOrdinal("SchoolClassId")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                IsPresent = reader.GetBoolean(reader.GetOrdinal("IsPresent"))
            });
        }

        return records;
    }

    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records)
    {
        var newRecords = records.ToList();
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existing = new Dictionary<int, (int Id, bool IsPresent)>();
        using (var select = new SqlCommand(
                   @"SELECT Id, StudentId, IsPresent 
                     FROM AttendanceRecords 
                     WHERE SchoolClassId = @classId AND [Date] = @date", connection, transaction))
        {
            select.Parameters.AddWithValue("@classId", classId);
            select.Parameters.AddWithValue("@date", date.Date);

            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                var studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                existing[studentId] = (reader.GetInt32(reader.GetOrdinal("Id")),
                    reader.GetBoolean(reader.GetOrdinal("IsPresent")));
            }
        }

        var lookup = newRecords.ToDictionary(r => r.StudentId, r => r.IsPresent);

        foreach (var kvp in existing)
        {
            if (lookup.TryGetValue(kvp.Key, out var isPresent))
            {
                using var update = new SqlCommand(
                    @"UPDATE AttendanceRecords 
                      SET IsPresent = @present 
                      WHERE Id = @id", connection, transaction);
                update.Parameters.AddWithValue("@present", isPresent);
                update.Parameters.AddWithValue("@id", kvp.Value.Id);
                update.ExecuteNonQuery();
            }
            else
            {
                using var delete = new SqlCommand("DELETE FROM AttendanceRecords WHERE Id = @id", connection, transaction);
                delete.Parameters.AddWithValue("@id", kvp.Value.Id);
                delete.ExecuteNonQuery();
            }
        }

        foreach (var record in newRecords)
        {
            if (existing.ContainsKey(record.StudentId))
            {
                continue;
            }

            using var insert = new SqlCommand(
                @"INSERT INTO AttendanceRecords (StudentId, SchoolClassId, [Date], IsPresent)
                  VALUES (@studentId, @classId, @date, @present)", connection, transaction);
            insert.Parameters.AddWithValue("@studentId", record.StudentId);
            insert.Parameters.AddWithValue("@classId", classId);
            insert.Parameters.AddWithValue("@date", date.Date);
            insert.Parameters.AddWithValue("@present", record.IsPresent);
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date)
    {
        const string sql = @"SELECT Id, StudentId, SchoolClassId, Assessment, Score, MaxScore, DateRecorded, Comments
                             FROM GradeRecords
                             WHERE SchoolClassId = @classId
                               AND Assessment = @assessment
                               AND DateRecorded = @date";

        var records = new List<GradeRecord>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);
        command.Parameters.AddWithValue("@assessment", assessment);
        command.Parameters.AddWithValue("@date", date.Date);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(MapGradeRecord(reader));
        }

        return records;
    }

    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records)
    {
        var newRecords = records.ToList();
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existing = new Dictionary<int, GradeRecord>();
        using (var select = new SqlCommand(
                   @"SELECT Id, StudentId, Score, MaxScore, Comments 
                     FROM GradeRecords
                     WHERE SchoolClassId = @classId AND Assessment = @assessment AND DateRecorded = @date",
                   connection, transaction))
        {
            select.Parameters.AddWithValue("@classId", classId);
            select.Parameters.AddWithValue("@assessment", assessment);
            select.Parameters.AddWithValue("@date", date.Date);

            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                var record = new GradeRecord
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                    Score = reader.IsDBNull(reader.GetOrdinal("Score"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("Score")),
                    MaxScore = reader.IsDBNull(reader.GetOrdinal("MaxScore"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("MaxScore")),
                    Comments = GetNullableString(reader, "Comments")
                };
                existing[record.StudentId] = record;
            }
        }

        var incomingLookup = newRecords.ToDictionary(r => r.StudentId, r => r);

        foreach (var kvp in existing)
        {
            if (incomingLookup.TryGetValue(kvp.Key, out var incoming) && incoming.Score.HasValue)
            {
                using var update = new SqlCommand(
                    @"UPDATE GradeRecords
                      SET Score = @score,
                          MaxScore = @maxScore,
                          Comments = @comments
                      WHERE Id = @id", connection, transaction);
                update.Parameters.AddWithValue("@score", incoming.Score.Value);
                update.Parameters.AddWithValue("@maxScore", (object?)maxScore ?? DBNull.Value);
                update.Parameters.AddWithValue("@comments", (object?)incoming.Comment?.Trim() ?? DBNull.Value);
                update.Parameters.AddWithValue("@id", kvp.Value.Id);
                update.ExecuteNonQuery();
            }
            else
            {
                using var delete = new SqlCommand("DELETE FROM GradeRecords WHERE Id = @id", connection, transaction);
                delete.Parameters.AddWithValue("@id", kvp.Value.Id);
                delete.ExecuteNonQuery();
            }
        }

        foreach (var incoming in newRecords)
        {
            if (existing.ContainsKey(incoming.StudentId) || !incoming.Score.HasValue)
            {
                continue;
            }

            using var insert = new SqlCommand(
                @"INSERT INTO GradeRecords (StudentId, SchoolClassId, Assessment, DateRecorded, Score, MaxScore, Comments)
                  VALUES (@studentId, @classId, @assessment, @date, @score, @maxScore, @comments)",
                connection, transaction);
            insert.Parameters.AddWithValue("@studentId", incoming.StudentId);
            insert.Parameters.AddWithValue("@classId", classId);
            insert.Parameters.AddWithValue("@assessment", assessment);
            insert.Parameters.AddWithValue("@date", date.Date);
            insert.Parameters.AddWithValue("@score", incoming.Score.Value);
            insert.Parameters.AddWithValue("@maxScore", (object?)maxScore ?? DBNull.Value);
            insert.Parameters.AddWithValue("@comments", (object?)incoming.Comment?.Trim() ?? DBNull.Value);
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public int GetClassCount(int teacherId)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand("SELECT COUNT(*) FROM Classes WHERE TeacherId = @teacherId", connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar() ?? 0);
    }

    public int GetDistinctStudentCount(int teacherId)
    {
        const string sql = @"SELECT COUNT(DISTINCT e.StudentId)
                             FROM Enrollments e
                             INNER JOIN Classes c ON e.SchoolClassId = c.Id
                             WHERE c.TeacherId = @teacherId";

        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar() ?? 0);
    }

    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take)
    {
        var events = new List<EventItem>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"SELECT Id, Title, Description, Location, Time, Day, Month, Year, UserId
              FROM TeacherEvents
              WHERE UserId = @userId", connection);
        command.Parameters.AddWithValue("@userId", userId);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(MapEvent(reader));
        }

        return events
            .Select(e => new { Event = e, Date = SafeBuildDate(e) })
            .Where(x => x.Date.HasValue)
            .OrderBy(x => x.Date!.Value)
            .ThenBy(x => x.Event.Time)
            .Take(take)
            .Select(x => x.Event)
            .ToList();
    }

    public List<GradeRecord> GetRecentGrades(int teacherId, int take)
    {
        const string sql = @"SELECT TOP (@take) g.Id, g.StudentId, g.SchoolClassId, g.Assessment, g.Score, g.MaxScore, g.DateRecorded, g.Comments,
                                    s.Id AS StudentId2, s.FirstName, s.LastName, s.Email,
                                    c.Id AS ClassId, c.Name, c.Room, c.Description, c.TeacherId
                             FROM GradeRecords g
                             INNER JOIN Students s ON g.StudentId = s.Id
                             INNER JOIN Classes c ON g.SchoolClassId = c.Id
                             WHERE c.TeacherId = @teacherId
                             ORDER BY g.DateRecorded DESC, g.Id DESC";

        var grades = new List<GradeRecord>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);
        command.Parameters.AddWithValue("@take", take);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var grade = MapGradeRecord(reader);
            grade.Student = MapStudent(reader);
            grade.SchoolClass = MapClass(reader, "ClassId");
            grades.Add(grade);
        }

        return grades;
    }

    public List<EventItem> GetEventsForMonth(int userId, int year, int month)
    {
        const string sql = @"SELECT Id, Title, Description, Location, Time, Day, Month, Year, UserId
                             FROM TeacherEvents
                             WHERE UserId = @userId AND Year = @year AND Month = @month
                             ORDER BY Day, Time";

        var events = new List<EventItem>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@year", year);
        command.Parameters.AddWithValue("@month", month);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(MapEvent(reader));
        }

        return events;
    }

    public EventItem? GetEvent(Guid id, int userId)
    {
        const string sql = @"SELECT Id, Title, Description, Location, Time, Day, Month, Year, UserId
                             FROM TeacherEvents
                             WHERE Id = @id AND UserId = @userId";

        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@userId", userId);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapEvent(reader) : null;
    }

    public void AddEvent(EventItem item)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"INSERT INTO TeacherEvents (Id, Title, Description, Location, Time, Day, Month, Year, UserId)
              VALUES (@id, @title, @description, @location, @time, @day, @month, @year, @userId)", connection);

        BindEventParameters(command, item);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void UpdateEvent(EventItem item)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(
            @"UPDATE TeacherEvents
              SET Title = @title,
                  Description = @description,
                  Location = @location,
                  Time = @time,
                  Day = @day,
                  Month = @month,
                  Year = @year
              WHERE Id = @id AND UserId = @userId", connection);

        BindEventParameters(command, item);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public void DeleteEvent(Guid id, int userId)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand("DELETE FROM TeacherEvents WHERE Id = @id AND UserId = @userId", connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@userId", userId);

        connection.Open();
        command.ExecuteNonQuery();
    }

    private List<SchoolClass> ReadSchoolClasses(string sql, params SqlParameter[] parameters)
    {
        var classes = new List<SchoolClass>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            classes.Add(MapClass(reader));
        }

        return classes;
    }

    private List<ClassEnrollment> LoadEnrollmentsForClass(int classId)
    {
        const string sql = @"SELECT e.Id, e.StudentId, e.SchoolClassId,
                                    s.FirstName, s.LastName, s.Email
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             WHERE e.SchoolClassId = @classId
                             ORDER BY s.LastName, s.FirstName";

        var enrollments = new List<ClassEnrollment>();
        using var connection = CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var student = MapStudent(reader);
            enrollments.Add(new ClassEnrollment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                SchoolClassId = reader.GetInt32(reader.GetOrdinal("SchoolClassId")),
                Student = student
            });
        }

        return enrollments;
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            Password = reader.GetString(reader.GetOrdinal("Password"))
        };
    }

    private static Student MapStudent(SqlDataReader reader)
    {
        var idOrdinal = reader.GetOrdinalSafe("StudentId2") ?? reader.GetOrdinal("Id");
        return new Student
        {
            Id = reader.GetInt32(idOrdinal),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = GetNullableString(reader, "Email")
        };
    }

    private static SchoolClass MapClass(SqlDataReader reader, string? idColumn = null)
    {
        return new SchoolClass
        {
            Id = reader.GetInt32(reader.GetOrdinal(idColumn ?? "Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Room = GetNullableString(reader, "Room"),
            Description = GetNullableString(reader, "Description"),
            TeacherId = reader.GetInt32(reader.GetOrdinal("TeacherId"))
        };
    }

    private static GradeRecord MapGradeRecord(SqlDataReader reader)
    {
        return new GradeRecord
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
            SchoolClassId = reader.GetInt32(reader.GetOrdinal("SchoolClassId")),
            Assessment = reader.GetString(reader.GetOrdinal("Assessment")),
            Score = reader.IsDBNull(reader.GetOrdinal("Score")) ? null : reader.GetDecimal(reader.GetOrdinal("Score")),
            MaxScore = reader.IsDBNull(reader.GetOrdinal("MaxScore")) ? null : reader.GetDecimal(reader.GetOrdinal("MaxScore")),
            DateRecorded = reader.GetDateTime(reader.GetOrdinal("DateRecorded")),
            Comments = GetNullableString(reader, "Comments")
        };
    }

    private static EventItem MapEvent(SqlDataReader reader)
    {
        return new EventItem
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = GetNullableString(reader, "Description"),
            Location = GetNullableString(reader, "Location"),
            Time = GetNullableString(reader, "Time"),
            Day = reader.GetInt32(reader.GetOrdinal("Day")),
            Month = reader.GetInt32(reader.GetOrdinal("Month")),
            Year = reader.GetInt32(reader.GetOrdinal("Year")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
        };
    }

    private static void BindEventParameters(SqlCommand command, EventItem item)
    {
        command.Parameters.AddWithValue("@id", item.Id);
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@description", (object?)item.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@location", (object?)item.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@time", (object?)item.Time ?? DBNull.Value);
        command.Parameters.AddWithValue("@day", item.Day);
        command.Parameters.AddWithValue("@month", item.Month);
        command.Parameters.AddWithValue("@year", item.Year);
        command.Parameters.AddWithValue("@userId", item.UserId);
    }

    private static DateTime? SafeBuildDate(EventItem e)
    {
        try
        {
            return new DateTime(e.Year, e.Month, e.Day);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}

internal static class DataReaderExtensions
{
    public static int? GetOrdinalSafe(this SqlDataReader reader, string column)
    {
        try
        {
            return reader.GetOrdinal(column);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
}
