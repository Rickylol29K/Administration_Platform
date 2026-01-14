using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date)
    {
        const string sql = @"SELECT Id, StudentId, SchoolClassId, Assessment, Score, MaxScore, DateRecorded, Comments
                             FROM GradeRecords
                             WHERE SchoolClassId = @classId
                               AND Assessment = @assessment
                               AND DateRecorded = @date";

        var records = new List<GradeRecord>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);
        command.Parameters.AddWithValue("@assessment", assessment);
        command.Parameters.AddWithValue("@date", date.Date);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(MapGradeRecord(reader));
        }

        return records;
    }

    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records)
    {
        var newRecords = records.ToList();
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        var existing = new Dictionary<int, GradeRecord>();
        using (var select = new SqlCommand(
                   @"SELECT Id, StudentId, Score, MaxScore, Comments 
                     FROM GradeRecords
                     WHERE SchoolClassId = @classId AND Assessment = @assessment AND DateRecorded = @date",
                   connection, transaction))
        {
            select.Parameters.AddWithValue("@classId", classId);
            select.Parameters.AddWithValue("@assessment", assessment);
            select.Parameters.AddWithValue("@date", date.Date);

            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                var record = new GradeRecord
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                    Score = reader.IsDBNull(reader.GetOrdinal("Score"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("Score")),
                    MaxScore = reader.IsDBNull(reader.GetOrdinal("MaxScore"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("MaxScore")),
                    Comments = GetNullableString(reader, "Comments")
                };
                existing[record.StudentId] = record;
            }
        }

        var validStudentIds = new HashSet<int>();
        using (var selectStudents = new SqlCommand(
                   @"SELECT e.StudentId
                     FROM Enrollments e
                     INNER JOIN Students s ON e.StudentId = s.Id
                     WHERE e.SchoolClassId = @classId", connection, transaction))
        {
            selectStudents.Parameters.AddWithValue("@classId", classId);
            using var reader = selectStudents.ExecuteReader();
            while (reader.Read())
            {
                validStudentIds.Add(reader.GetInt32(reader.GetOrdinal("StudentId")));
            }
        }

        var incomingLookup = new Dictionary<int, (decimal? Score, string? Comment)>();
        foreach (var record in newRecords)
        {
            if (record.StudentId <= 0 || !validStudentIds.Contains(record.StudentId))
            {
                continue;
            }

            incomingLookup[record.StudentId] = (record.Score, record.Comment);
        }

        foreach (var kvp in existing)
        {
            if (incomingLookup.TryGetValue(kvp.Key, out var incoming) && incoming.Score.HasValue)
            {
                using var update = new SqlCommand(
                    @"UPDATE GradeRecords
                      SET Score = @score,
                          MaxScore = @maxScore,
                          Comments = @comments
                      WHERE Id = @id", connection, transaction);
                update.Parameters.AddWithValue("@score", incoming.Score.Value);
                update.Parameters.AddWithValue("@maxScore", (object?)maxScore ?? DBNull.Value);
                update.Parameters.AddWithValue("@comments", (object?)incoming.Comment?.Trim() ?? DBNull.Value);
                update.Parameters.AddWithValue("@id", kvp.Value.Id);
                update.ExecuteNonQuery();
            }
            else
            {
                using var delete = new SqlCommand("DELETE FROM GradeRecords WHERE Id = @id", connection, transaction);
                delete.Parameters.AddWithValue("@id", kvp.Value.Id);
                delete.ExecuteNonQuery();
            }
        }

        foreach (var incoming in newRecords)
        {
            if (incoming.StudentId <= 0 || !validStudentIds.Contains(incoming.StudentId))
            {
                continue;
            }

            if (existing.ContainsKey(incoming.StudentId) || !incoming.Score.HasValue)
            {
                continue;
            }

            using var insert = new SqlCommand(
                @"INSERT INTO GradeRecords (StudentId, SchoolClassId, Assessment, DateRecorded, Score, MaxScore, Comments)
                  VALUES (@studentId, @classId, @assessment, @date, @score, @maxScore, @comments)",
                connection, transaction);
            insert.Parameters.AddWithValue("@studentId", incoming.StudentId);
            insert.Parameters.AddWithValue("@classId", classId);
            insert.Parameters.AddWithValue("@assessment", assessment);
            insert.Parameters.AddWithValue("@date", date.Date);
            insert.Parameters.AddWithValue("@score", incoming.Score.Value);
            insert.Parameters.AddWithValue("@maxScore", (object?)maxScore ?? DBNull.Value);
            insert.Parameters.AddWithValue("@comments", (object?)incoming.Comment?.Trim() ?? DBNull.Value);
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public List<GradeRecord> GetRecentGrades(int teacherId, int take)
    {
        const string sql = @"SELECT TOP (@take) g.Id, g.StudentId, g.SchoolClassId, g.Assessment, g.Score, g.MaxScore, g.DateRecorded, g.Comments,
                                    s.Id AS StudentId2, s.FirstName, s.LastName, s.Email,
                                    c.Id AS ClassId, c.Name, c.Room, c.Description, c.TeacherId
                             FROM GradeRecords g
                             INNER JOIN Students s ON g.StudentId = s.Id
                             INNER JOIN Classes c ON g.SchoolClassId = c.Id
                             WHERE c.TeacherId = @teacherId
                             ORDER BY g.DateRecorded DESC, g.Id DESC";

        var grades = new List<GradeRecord>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@teacherId", teacherId);
        command.Parameters.AddWithValue("@take", take);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var grade = MapGradeRecord(reader);
            grade.Student = MapStudent(reader);
            grade.SchoolClass = MapClass(reader, "ClassId");
            grades.Add(grade);
        }

        return grades;
    }
}
