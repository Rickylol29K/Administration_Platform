using AdministrationPlat.Models;
using Microsoft.Data.SqlClient;

namespace DAL;

public partial class SqlDataRepository
{
    public Student? GetStudentByEmail(string email)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT Id, FirstName, LastName, Email 
                             FROM Students 
                             WHERE Email = @email";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapStudent(reader) : null;
    }

    public Student AddStudent(Student student)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Students (FirstName, LastName, Email)
                             VALUES (@first, @last, @email);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@first", student.FirstName);
        command.Parameters.AddWithValue("@last", student.LastName);
        command.Parameters.AddWithValue("@email", (object?)student.Email ?? DBNull.Value);

        student.Id = (int)(command.ExecuteScalar() ?? 0);
        return student;
    }

    public bool EnrollmentExists(int studentId, int classId)
    {
        using var connection = OpenConnection();
        const string sql = @"SELECT 1 FROM Enrollments 
                             WHERE StudentId = @studentId AND SchoolClassId = @classId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@studentId", studentId);
        command.Parameters.AddWithValue("@classId", classId);

        var exists = command.ExecuteScalar();
        return exists != null && exists != DBNull.Value;
    }

    public void AddEnrollment(int studentId, int classId)
    {
        using var connection = OpenConnection();
        const string sql = @"INSERT INTO Enrollments (StudentId, SchoolClassId) 
                             VALUES (@studentId, @classId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@studentId", studentId);
        command.Parameters.AddWithValue("@classId", classId);

        command.ExecuteNonQuery();
    }

    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId)
    {
        const string sql = @"SELECT e.Id, e.StudentId, e.SchoolClassId,
                                    s.FirstName, s.LastName, s.Email,
                                    c.Id AS ClassId, c.Name, c.Room, c.Description, c.TeacherId
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             INNER JOIN Classes c ON e.SchoolClassId = c.Id
                             WHERE e.Id = @enrollmentId AND c.TeacherId = @teacherId";

        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@enrollmentId", enrollmentId);
        command.Parameters.AddWithValue("@teacherId", teacherId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ClassEnrollment
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
            SchoolClassId = reader.GetInt32(reader.GetOrdinal("SchoolClassId")),
            Student = MapStudent(reader),
            SchoolClass = MapClass(reader, "ClassId")
        };
    }

    public void RemoveEnrollment(int enrollmentId)
    {
        using var connection = OpenConnection();
        const string sql = "DELETE FROM Enrollments WHERE Id = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", enrollmentId);

        command.ExecuteNonQuery();
    }

    public List<Student> GetStudentsForClass(int classId)
    {
        const string sql = @"SELECT s.Id, s.FirstName, s.LastName, s.Email
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             WHERE e.SchoolClassId = @classId
                             ORDER BY s.LastName, s.FirstName";

        var students = new List<Student>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            students.Add(MapStudent(reader));
        }

        return students;
    }

    private List<ClassEnrollment> LoadEnrollmentsForClass(int classId)
    {
        const string sql = @"SELECT e.Id, e.StudentId, e.SchoolClassId,
                                    s.FirstName, s.LastName, s.Email
                             FROM Enrollments e
                             INNER JOIN Students s ON e.StudentId = s.Id
                             WHERE e.SchoolClassId = @classId
                             ORDER BY s.LastName, s.FirstName";

        var enrollments = new List<ClassEnrollment>();
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@classId", classId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var student = MapStudent(reader);
            enrollments.Add(new ClassEnrollment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                SchoolClassId = reader.GetInt32(reader.GetOrdinal("SchoolClassId")),
                Student = student
            });
        }

        return enrollments;
    }
}
