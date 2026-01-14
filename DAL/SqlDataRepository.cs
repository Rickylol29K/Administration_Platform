using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public class SqlDataRepository : IDataRepository
{
    private readonly string _connectionString;

    // Sql data repository.
    public SqlDataRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    // Get user.
    public User? GetUser(string username, string password)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT Id, Username, Password, IsAdmin
                             FROM Users 
                             WHERE Username = @username AND Password = @password";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapUser(reader);
        }

        return null;
    }

    // Get user by id.
    public User? GetUserById(int id)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT Id, Username, Password, IsAdmin
                             FROM Users
                             WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapUser(reader);
        }

        return null;
    }

    // Username exists.
    public bool UsernameExists(string username)
    {
        using var connection = OpenConnection();
        const string sql = "SELECT 1 FROM Users WHERE Username = @username";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        var exists = command.ExecuteScalar();
        return exists != null && exists != DBNull.Value;
    }

    // Create user.
    public User CreateUser(string username, string password, bool isAdmin)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Users (Username, Password, IsAdmin) 
                             VALUES (@username, @password, @isAdmin);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);
        command.Parameters.AddWithValue("@isAdmin", isAdmin);

        var id = (int)(command.ExecuteScalar() ?? 0);
        return new User { Id = id, Username = username, Password = password, IsAdmin = isAdmin };
    }

    // Get teachers.
    public List<User> GetTeachers()
    {
        const string sql = @"SELECT Id, Username, Password, IsAdmin
                             FROM Users
                             WHERE IsAdmin = 0
                             ORDER BY Username";

        var teachers = new List<User>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            teachers.Add(MapUser(reader));
        }

        return teachers;
    }

    // Get classes for teacher.
    public List<SchoolClass> GetClassesForTeacher(int teacherId)
    {
        const string sql = @"SELECT Id, Name, Room, Description, TeacherId
                             FROM Classes
                             WHERE TeacherId = @teacherId
                             ORDER BY Name";

        return ReadSchoolClasses(sql, new SqlParameter("@teacherId", teacherId));
    }

    // Get all classes.
    public List<SchoolClass> GetAllClasses()
    {
        const string sql = @"SELECT Id, Name, Room, Description, TeacherId
                             FROM Classes
                             ORDER BY Name";

        return ReadSchoolClasses(sql);
    }

    // Add class.
    public SchoolClass AddClass(SchoolClass schoolClass)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Classes (Name, Room, Description, TeacherId)
                             VALUES (@name, @room, @description, @teacherId);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", schoolClass.Name);
        command.Parameters.AddWithValue("@room", (object?)schoolClass.Room ?? DBNull.Value);
        command.Parameters.AddWithValue("@description", (object?)schoolClass.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@teacherId", schoolClass.TeacherId);

        schoolClass.Id = (int)(command.ExecuteScalar() ?? 0);
        return schoolClass;
    }

    // Get class with enrollments.
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

    // Get class name.
    public string? GetClassName(int classId)
    {
        using var connection = OpenConnection();
        const string sql = "SELECT Name FROM Classes WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", classId);

        var result = command.ExecuteScalar();
        return result as string;
    }

    // Get class count.
    public int GetClassCount(int teacherId)
    {
        using var connection = OpenConnection();
        const string sql = "SELECT COUNT(*) FROM Classes WHERE TeacherId = @teacherId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        return Convert.ToInt32(command.ExecuteScalar() ?? 0);
    }

    // Get distinct student count.
    public int GetDistinctStudentCount(int teacherId)
    {
        const string sql = @"SELECT COUNT(DISTINCT e.StudentId)
                             FROM Enrollments e
                             INNER JOIN Classes c ON e.SchoolClassId = c.Id
                             WHERE c.TeacherId = @teacherId";

        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        return Convert.ToInt32(command.ExecuteScalar() ?? 0);
    }

    // Get student by email.
    public Student? GetStudentByEmail(string email)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT Id, FirstName, LastName, Email 
                             FROM Students 
                             WHERE Email = @email";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapStudent(reader);
        }

        return null;
    }

    // Add student.
    public Student AddStudent(Student student)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Students (FirstName, LastName, Email)
                             VALUES (@first, @last, @email);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@first", student.FirstName);
        command.Parameters.AddWithValue("@last", student.LastName);
        command.Parameters.AddWithValue("@email", (object?)student.Email ?? DBNull.Value);

        student.Id = (int)(command.ExecuteScalar() ?? 0);
        return student;
    }

    // Enrollment exists.
    public bool EnrollmentExists(int studentId, int classId)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT 1 FROM Enrollments 
                             WHERE StudentId = @studentId AND SchoolClassId = @classId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@studentId", studentId);
        command.Parameters.AddWithValue("@classId", classId);

        var exists = command.ExecuteScalar();
        return exists != null && exists != DBNull.Value;
    }

    // Add enrollment.
    public void AddEnrollment(int studentId, int classId)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Enrollments (StudentId, SchoolClassId) 
                             VALUES (@studentId, @classId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@studentId", studentId);
        command.Parameters.AddWithValue("@classId", classId);

        command.ExecuteNonQuery();
    }

    // Get enrollment with details.
    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId)
    {
        const string sql = @"SELECT e.Id, e.StudentId, e.SchoolClassId,
                                    s.FirstName, s.LastName, s.Email,
                                    c.Id AS ClassId, c.Name, c.Room, c.Description, c.TeacherId
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             INNER JOIN Classes c ON e.SchoolClassId = c.Id
                             WHERE e.Id = @enrollmentId AND c.TeacherId = @teacherId";

        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@enrollmentId", enrollmentId);
        command.Parameters.AddWithValue("@teacherId", teacherId);

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

    // Remove enrollment.
    public void RemoveEnrollment(int enrollmentId)
    {
        using var connection = OpenConnection();
        const string sql = "DELETE FROM Enrollments WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", enrollmentId);

        command.ExecuteNonQuery();
    }

    // Get students for class.
    public List<Student> GetStudentsForClass(int classId)
    {
        const string sql = @"SELECT s.Id, s.FirstName, s.LastName, s.Email
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             WHERE e.SchoolClassId = @classId
                             ORDER BY s.LastName, s.FirstName";

        var students = new List<Student>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            students.Add(MapStudent(reader));
        }

        return students;
    }

    // Get attendance records.
    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date)
    {
        const string sql = @"SELECT Id, StudentId, SchoolClassId, [Date], IsPresent
                             FROM AttendanceRecords
                             WHERE SchoolClassId = @classId AND [Date] = @date";

        var records = new List<AttendanceRecord>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);
        command.Parameters.AddWithValue("@date", date.Date);

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

    // Save attendance records.
    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records)
    {
        var newRecords = records.ToList();
        using var connection = OpenConnection();
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

        var lookup = new Dictionary<int, bool>();
        foreach (var record in newRecords)
        {
            lookup[record.StudentId] = record.IsPresent;
        }

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

    // Get grade records.
    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date)
    {
        const string sql = @"SELECT Id, StudentId, SchoolClassId, Assessment, Score, MaxScore, DateRecorded, Comments
                             FROM GradeRecords
                             WHERE SchoolClassId = @classId
                               AND Assessment = @assessment
                               AND DateRecorded = @date";

        var records = new List<GradeRecord>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);
        command.Parameters.AddWithValue("@assessment", assessment);
        command.Parameters.AddWithValue("@date", date.Date);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(MapGradeRecord(reader));
        }

        return records;
    }

    // Save grade records.
    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records)
    {
        var newRecords = records.ToList();
        using var connection = OpenConnection();
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

        var incomingLookup = new Dictionary<int, (decimal? Score, string? Comment)>();
        foreach (var record in newRecords)
        {
            incomingLookup[record.StudentId] = (record.Score, record.Comment);
        }

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

    // Get recent grades.
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
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);
        command.Parameters.AddWithValue("@take", take);

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

    // Get upcoming events.
    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take)
    {
        var events = new List<EventItem>();
        using var connection = OpenConnection();
        const string sql = @"SELECT Id, Title, Description, Location, Time, Day, Month, Year, UserId
                             FROM TeacherEvents
                             WHERE UserId = @userId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(MapEvent(reader));
        }

        var eventsWithDates = new List<(EventItem Item, DateTime Date)>();
        foreach (var evt in events)
        {
            var date = SafeBuildDate(evt);
            if (date.HasValue)
            {
                eventsWithDates.Add((evt, date.Value));
            }
        }

        eventsWithDates.Sort((first, second) =>
        {
            var compareDate = DateTime.Compare(first.Date, second.Date);
            if (compareDate != 0)
            {
                return compareDate;
            }

            return string.Compare(first.Item.Time, second.Item.Time, StringComparison.Ordinal);
        });

        var upcoming = new List<EventItem>();
        foreach (var evt in eventsWithDates)
        {
            if (upcoming.Count >= take)
            {
                break;
            }

            upcoming.Add(evt.Item);
        }

        return upcoming;
    }

    // Get events for month.
    public List<EventItem> GetEventsForMonth(int userId, int year, int month)
    {
        const string sql = @"SELECT Id, Title, Description, Location, Time, Day, Month, Year, UserId
                             FROM TeacherEvents
                             WHERE UserId = @userId AND Year = @year AND Month = @month
                             ORDER BY Day, Time";

        var events = new List<EventItem>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@year", year);
        command.Parameters.AddWithValue("@month", month);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(MapEvent(reader));
        }

        return events;
    }

    // Get event.
    public EventItem? GetEvent(Guid id, int userId)
    {
        const string sql = @"SELECT Id, Title, Description, Location, Time, Day, Month, Year, UserId
                             FROM TeacherEvents
                             WHERE Id = @id AND UserId = @userId";

        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@userId", userId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapEvent(reader);
        }

        return null;
    }

    // Add event.
    public void AddEvent(EventItem item)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO TeacherEvents (Id, Title, Description, Location, Time, Day, Month, Year, UserId)
                             VALUES (@id, @title, @description, @location, @time, @day, @month, @year, @userId)";

        using var command = new SqlCommand(sql, connection);
        BindEventParameters(command, item);
        command.ExecuteNonQuery();
    }

    // Update event.
    public void UpdateEvent(EventItem item)
    {
        using var connection = OpenConnection();
        const string sql = @"UPDATE TeacherEvents
                             SET Title = @title,
                                 Description = @description,
                                 Location = @location,
                                 Time = @time,
                                 Day = @day,
                                 Month = @month,
                                 Year = @year
                             WHERE Id = @id AND UserId = @userId";

        using var command = new SqlCommand(sql, connection);
        BindEventParameters(command, item);
        command.ExecuteNonQuery();
    }

    // Delete event.
    public void DeleteEvent(Guid id, int userId)
    {
        using var connection = OpenConnection();
        const string sql = "DELETE FROM TeacherEvents WHERE Id = @id AND UserId = @userId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@userId", userId);

        command.ExecuteNonQuery();
    }

    // Get announcements.
    public List<Announcement> GetAnnouncements(int take)
    {
        const string sql = @"SELECT TOP (@take) Id, Title, Body, CreatedAt, CreatedByUserId
                             FROM Announcements
                             ORDER BY CreatedAt DESC";

        var announcements = new List<Announcement>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@take", take);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            announcements.Add(MapAnnouncement(reader));
        }

        return announcements;
    }

    // Get all announcements.
    public List<Announcement> GetAllAnnouncements()
    {
        const string sql = @"SELECT Id, Title, Body, CreatedAt, CreatedByUserId
                             FROM Announcements
                             ORDER BY CreatedAt DESC";

        var announcements = new List<Announcement>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            announcements.Add(MapAnnouncement(reader));
        }

        return announcements;
    }

    // Get announcement.
    public Announcement? GetAnnouncement(Guid id)
    {
        const string sql = @"SELECT Id, Title, Body, CreatedAt, CreatedByUserId
                             FROM Announcements
                             WHERE Id = @id";

        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAnnouncement(reader);
        }

        return null;
    }

    // Add announcement.
    public Announcement AddAnnouncement(Announcement announcement)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Announcements (Id, Title, Body, CreatedAt, CreatedByUserId)
                             VALUES (@id, @title, @body, @createdAt, @createdByUserId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", announcement.Id);
        command.Parameters.AddWithValue("@title", announcement.Title);
        command.Parameters.AddWithValue("@body", (object?)announcement.Body ?? DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", announcement.CreatedAt);
        command.Parameters.AddWithValue("@createdByUserId", announcement.CreatedByUserId);

        command.ExecuteNonQuery();
        return announcement;
    }

    // Delete announcement.
    public void DeleteAnnouncement(Guid id)
    {
        using var connection = OpenConnection();
        const string sql = "DELETE FROM Announcements WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    // Open connection.
    private SqlConnection OpenConnection()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    // Create connection.
    private SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    // Read school classes.
    private List<SchoolClass> ReadSchoolClasses(string sql, params SqlParameter[] parameters)
    {
        var classes = new List<SchoolClass>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            classes.Add(MapClass(reader));
        }

        return classes;
    }

    // Load enrollments for class.
    private List<ClassEnrollment> LoadEnrollmentsForClass(int classId)
    {
        const string sql = @"SELECT e.Id, e.StudentId, e.SchoolClassId,
                                    s.FirstName, s.LastName, s.Email
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             WHERE e.SchoolClassId = @classId
                             ORDER BY s.LastName, s.FirstName";

        var enrollments = new List<ClassEnrollment>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);

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

    // Map user.
    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            Password = reader.GetString(reader.GetOrdinal("Password")),
            IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin"))
        };
    }

    // Map student.
    private static Student MapStudent(SqlDataReader reader)
    {
        int? idOrdinal = reader.GetOrdinalSafe("StudentId2");
        int columnOrdinal = idOrdinal.HasValue ? idOrdinal.Value : reader.GetOrdinal("Id");
        return new Student
        {
            Id = reader.GetInt32(columnOrdinal),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = GetNullableString(reader, "Email")
        };
    }

    // Map class.
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

    // Map grade record.
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

    // Map event.
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

    // Map announcement.
    private static Announcement MapAnnouncement(SqlDataReader reader)
    {
        return new Announcement
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Body = GetNullableString(reader, "Body"),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            CreatedByUserId = reader.GetInt32(reader.GetOrdinal("CreatedByUserId"))
        };
    }

    // Bind event parameters.
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

    // Safe build date.
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

    // Get nullable string.
    private static string? GetNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
