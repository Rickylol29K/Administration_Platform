using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public Student? GetStudentByEmail(string email) => _classes.GetStudentByEmail(email);

    public Student AddStudent(Student student) => _classes.AddStudent(student);

    public bool EnrollmentExists(int studentId, int classId) => _classes.EnrollmentExists(studentId, classId);

    public void AddEnrollment(int studentId, int classId) => _classes.AddEnrollment(studentId, classId);

    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId) =>
        _classes.GetEnrollmentWithDetails(enrollmentId, teacherId);

    public void RemoveEnrollment(int enrollmentId) => _classes.RemoveEnrollment(enrollmentId);

    public List<Student> GetStudentsForClass(int classId) => _classes.GetStudentsForClass(classId);

    public ClassMembershipResult AddStudentToClass(int teacherId, int classId, string firstName, string lastName, string? email) =>
        _classes.AddStudentToClass(teacherId, classId, firstName, lastName, email);

    public ClassMembershipResult AddStudentToClassAsAdmin(int classId, string firstName, string lastName, string? email) =>
        _classes.AddStudentToClassAsAdmin(classId, firstName, lastName, email);

    public ClassMembershipResult RemoveStudentFromClass(int teacherId, int enrollmentId) =>
        _classes.RemoveStudentFromClass(teacherId, enrollmentId);
}
