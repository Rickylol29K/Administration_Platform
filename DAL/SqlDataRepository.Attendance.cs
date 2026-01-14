using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
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
}
