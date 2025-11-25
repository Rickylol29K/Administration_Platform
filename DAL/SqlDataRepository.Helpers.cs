using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
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
