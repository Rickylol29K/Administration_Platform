using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository : IDataRepository
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

    private SqlConnection OpenConnection()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    private SqlConnection CreateConnection() => new(_connectionString);
}
