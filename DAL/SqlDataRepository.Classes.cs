using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
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
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Classes (Name, Room, Description, TeacherId)
                             VALUES (@name, @room, @description, @teacherId);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", schoolClass.Name);
        object? roomValue = schoolClass.Room;
        if (roomValue == null)
        {
            roomValue = DBNull.Value;
        }
        command.Parameters.AddWithValue("@room", roomValue);

        object? descriptionValue = schoolClass.Description;
        if (descriptionValue == null)
        {
            descriptionValue = DBNull.Value;
        }
        command.Parameters.AddWithValue("@description", descriptionValue);
        command.Parameters.AddWithValue("@teacherId", schoolClass.TeacherId);

        object? scalar = command.ExecuteScalar();
        if (scalar == null || scalar == DBNull.Value)
        {
            schoolClass.Id = 0;
        }
        else
        {
            schoolClass.Id = (int)scalar;
        }
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

    public string? GetClassName(int classId)
    {
        using var connection = OpenConnection();
        const string sql = "SELECT Name FROM Classes WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", classId);

        var result = command.ExecuteScalar();
        return result as string;
    }

    public int GetClassCount(int teacherId)
    {
        using var connection = OpenConnection();
        const string sql = "SELECT COUNT(*) FROM Classes WHERE TeacherId = @teacherId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        object? scalar = command.ExecuteScalar();
        if (scalar == null || scalar == DBNull.Value)
        {
            return 0;
        }

        return Convert.ToInt32(scalar);
    }

    public int GetDistinctStudentCount(int teacherId)
    {
        const string sql = @"SELECT COUNT(DISTINCT e.StudentId)
                             FROM Enrollments e
                             INNER JOIN Classes c ON e.SchoolClassId = c.Id
                             WHERE c.TeacherId = @teacherId";

        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        object? scalar = command.ExecuteScalar();
        if (scalar == null || scalar == DBNull.Value)
        {
            return 0;
        }

        return Convert.ToInt32(scalar);
    }
}
