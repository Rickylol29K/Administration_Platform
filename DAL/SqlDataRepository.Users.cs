using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
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

    public bool UsernameExists(string username)
    {
        using var connection = OpenConnection();
        const string sql = "SELECT 1 FROM Users WHERE Username = @username";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        var exists = command.ExecuteScalar();
        return exists != null && exists != DBNull.Value;
    }

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
}
