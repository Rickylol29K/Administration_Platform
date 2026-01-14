using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
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

    public void AddEvent(EventItem item)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO TeacherEvents (Id, Title, Description, Location, Time, Day, Month, Year, UserId)
                             VALUES (@id, @title, @description, @location, @time, @day, @month, @year, @userId)";

        using var command = new SqlCommand(sql, connection);
        BindEventParameters(command, item);
        command.ExecuteNonQuery();
    }

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

    public void DeleteEvent(Guid id, int userId)
    {
        using var connection = OpenConnection();
        const string sql = "DELETE FROM TeacherEvents WHERE Id = @id AND UserId = @userId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@userId", userId);

        command.ExecuteNonQuery();
    }
}
