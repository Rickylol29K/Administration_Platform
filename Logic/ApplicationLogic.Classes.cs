using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<SchoolClass> GetClassesForTeacher(int teacherId) => _classes.GetClassesForTeacher(teacherId);

    public List<SchoolClass> GetAllClasses() => _classes.GetAllClasses();

    public SchoolClass AddClass(SchoolClass schoolClass) => _classes.AddClass(schoolClass);

    public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null) => _classes.GetClassWithEnrollments(classId, teacherId);

    public string? GetClassName(int classId) => _classes.GetClassName(classId);

    public int GetClassCount(int teacherId) => _classes.GetClassCount(teacherId);

    public int GetDistinctStudentCount(int teacherId) => _classes.GetDistinctStudentCount(teacherId);

    public OperationResult<SchoolClass> CreateClass(int teacherId, string name, string? room, string? description) =>
        _classes.CreateClass(teacherId, name, room, description);

    public OperationResult<ClassOverlay> LoadClassOverlay(int classId, int? teacherId = null) =>
        _classes.LoadClassOverlay(classId, teacherId);
}
