using AdministrationPlat.Models;
using DAL;
using Logic.Contracts;
using Logic.Models;

namespace Logic.Services;

internal sealed class ClassLogic : IClassLogic
{
    private readonly IDataRepository _repository;

    public ClassLogic(IDataRepository repository)
    {
        _repository = repository;
    }

    public List<SchoolClass> GetClassesForTeacher(int teacherId) => _repository.GetClassesForTeacher(teacherId);

    public List<SchoolClass> GetAllClasses() => _repository.GetAllClasses();

    public SchoolClass AddClass(SchoolClass schoolClass) => _repository.AddClass(schoolClass);

    public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null) => _repository.GetClassWithEnrollments(classId, teacherId);

    public string? GetClassName(int classId) => _repository.GetClassName(classId);

    public int GetClassCount(int teacherId) => _repository.GetClassCount(teacherId);

    public int GetDistinctStudentCount(int teacherId) => _repository.GetDistinctStudentCount(teacherId);

    public OperationResult<SchoolClass> CreateClass(int teacherId, string name, string? room, string? description)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return OperationResult<SchoolClass>.Fail("Class name is required.");
        }

        var schoolClass = new SchoolClass
        {
            Name = trimmedName,
            Room = string.IsNullOrWhiteSpace(room) ? null : room.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            TeacherId = teacherId
        };

        var created = _repository.AddClass(schoolClass);
        return OperationResult<SchoolClass>.Ok(created);
    }

    public OperationResult<ClassOverlay> LoadClassOverlay(int classId, int? teacherId = null)
    {
        var cls = _repository.GetClassWithEnrollments(classId, teacherId);
        if (cls == null)
        {
            return OperationResult<ClassOverlay>.Fail("Unable to load the requested class.");
        }

        var enrollments = cls.Enrollments
            .Where(e => e.Student != null)
            .OrderBy(e => e.Student!.LastName)
            .ThenBy(e => e.Student!.FirstName)
            .ToList();

        var overlay = new ClassOverlay
        {
            ActiveClass = cls,
            Enrollments = enrollments
        };

        return OperationResult<ClassOverlay>.Ok(overlay);
    }

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
