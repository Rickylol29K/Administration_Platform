using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
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

    public Announcement AddAnnouncement(Announcement announcement)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Announcements (Id, Title, Body, CreatedAt, CreatedByUserId)
                             VALUES (@id, @title, @body, @createdAt, @createdByUserId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", announcement.Id);
        command.Parameters.AddWithValue("@title", announcement.Title);
        object? bodyValue = announcement.Body;
        if (bodyValue == null)
        {
            bodyValue = DBNull.Value;
        }
        command.Parameters.AddWithValue("@body", bodyValue);
        command.Parameters.AddWithValue("@createdAt", announcement.CreatedAt);
        command.Parameters.AddWithValue("@createdByUserId", announcement.CreatedByUserId);

        command.ExecuteNonQuery();
        return announcement;
    }

    public void DeleteAnnouncement(Guid id)
    {
        using var connection = OpenConnection();
        const string sql = "DELETE FROM Announcements WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }
}
