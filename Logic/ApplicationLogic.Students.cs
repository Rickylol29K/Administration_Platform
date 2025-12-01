using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public Student? GetStudentByEmail(string email) => _repository.GetStudentByEmail(email);

    public Student AddStudent(Student student) => _repository.AddStudent(student);

    public bool EnrollmentExists(int studentId, int classId) => _repository.EnrollmentExists(studentId, classId);

    public void AddEnrollment(int studentId, int classId) => _repository.AddEnrollment(studentId, classId);

    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId) => _repository.GetEnrollmentWithDetails(enrollmentId, teacherId);

    public void RemoveEnrollment(int enrollmentId) => _repository.RemoveEnrollment(enrollmentId);

    public List<Student> GetStudentsForClass(int classId) => _repository.GetStudentsForClass(classId);

    public ClassMembershipResult AddStudentToClass(int teacherId, int classId, string firstName, string lastName, string? email)
    {
        var overlayResult = LoadClassOverlay(classId, teacherId);
        if (!overlayResult.Success || overlayResult.Value?.ActiveClass == null)
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Class not found."
            };
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Student first and last name are required.",
                Overlay = overlayResult.Value
            };
        }

        Student? student = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            student = _repository.GetStudentByEmail(email.Trim());
        }

        if (student == null)
        {
            student = new Student
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
            };
            _repository.AddStudent(student);
        }

        var alreadyEnrolled = _repository.EnrollmentExists(student.Id, classId);
        if (!alreadyEnrolled)
        {
            _repository.AddEnrollment(student.Id, classId);
        }

        var refreshedOverlay = LoadClassOverlay(classId, teacherId).Value ?? overlayResult.Value;
        var message = alreadyEnrolled
            ? $"{student.FullName} is already enrolled in this class."
            : $"{student.FullName} added to {overlayResult.Value.ActiveClass?.Name}.";

        return new ClassMembershipResult
        {
            Success = true,
            Message = message,
            Overlay = refreshedOverlay,
            AlreadyEnrolled = alreadyEnrolled
        };
    }

    public ClassMembershipResult RemoveStudentFromClass(int teacherId, int enrollmentId)
    {
        var enrollment = _repository.GetEnrollmentWithDetails(enrollmentId, teacherId);

        if (enrollment == null)
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Enrollment not found."
            };
        }

        _repository.RemoveEnrollment(enrollmentId);

        var refreshedOverlay = LoadClassOverlay(enrollment.SchoolClassId, teacherId).Value;
        var message = $"{enrollment.Student?.FullName ?? "Student"} removed from {enrollment.SchoolClass?.Name}.";

        return new ClassMembershipResult
        {
            Success = true,
            Message = message,
            Overlay = refreshedOverlay ?? new ClassOverlay { ActiveClass = enrollment.SchoolClass },
            AlreadyEnrolled = false
        };
    }
}
