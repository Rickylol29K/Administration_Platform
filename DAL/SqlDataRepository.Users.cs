using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
    public User? GetUser(string username, string password)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT Id, Username, Password 
                             FROM Users 
                             WHERE Username = @username AND Password = @password";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
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

    public User CreateUser(string username, string password)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Users (Username, Password) 
                             VALUES (@username, @password);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        var id = (int)(command.ExecuteScalar() ?? 0);
        return new User { Id = id, Username = username, Password = password };
    }
}
