using AdministrationPlat.Models;
using Logic.Models;

namespace Logic.Contracts;

public interface IClassLogic
{
    List<SchoolClass> GetClassesForTeacher(int teacherId);
    List<SchoolClass> GetAllClasses();
    SchoolClass AddClass(SchoolClass schoolClass);
    SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null);
    string? GetClassName(int classId);
    OperationResult<SchoolClass> CreateClass(int teacherId, string name, string? room, string? description);
    OperationResult<ClassOverlay> LoadClassOverlay(int classId, int? teacherId = null);
    ClassMembershipResult AddStudentToClass(int teacherId, int classId, string firstName, string lastName, string? email);
    ClassMembershipResult RemoveStudentFromClass(int teacherId, int enrollmentId);

    Student? GetStudentByEmail(string email);
    Student AddStudent(Student student);
    bool EnrollmentExists(int studentId, int classId);
    void AddEnrollment(int studentId, int classId);
    ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId);
    void RemoveEnrollment(int enrollmentId);
    List<Student> GetStudentsForClass(int classId);

    int GetClassCount(int teacherId);
    int GetDistinctStudentCount(int teacherId);
}
