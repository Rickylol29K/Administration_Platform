using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
    private SqlConnection OpenConnection()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    private SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

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
