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
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using (var delete = new SqlCommand(
                   @"DELETE FROM AttendanceRecords
                     WHERE SchoolClassId = @classId AND [Date] = @date", connection, transaction))
        {
            delete.Parameters.AddWithValue("@classId", classId);
            delete.Parameters.AddWithValue("@date", date.Date);
            delete.ExecuteNonQuery();
        }

        foreach (var record in records)
        {
            if (record.StudentId <= 0)
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
