using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
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
}
