using AdministrationPlat.Models;

namespace DAL;

public interface IDataRepository
{
    User? GetUser(string username, string password);
    bool UsernameExists(string username);
    User CreateUser(string username, string password);

    List<SchoolClass> GetClassesForTeacher(int teacherId);
    List<SchoolClass> GetAllClasses();
    SchoolClass AddClass(SchoolClass schoolClass);
    SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null);

    Student? GetStudentByEmail(string email);
    Student AddStudent(Student student);
    bool EnrollmentExists(int studentId, int classId);
    void AddEnrollment(int studentId, int classId);
    ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId);
    void RemoveEnrollment(int enrollmentId);

    string? GetClassName(int classId);
    List<Student> GetStudentsForClass(int classId);

    List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date);
    void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records);

    List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date);
    void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records);

    int GetClassCount(int teacherId);
    int GetDistinctStudentCount(int teacherId);
    List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take);
    List<GradeRecord> GetRecentGrades(int teacherId, int take);

    List<EventItem> GetEventsForMonth(int userId, int year, int month);
    EventItem? GetEvent(Guid id, int userId);
    void AddEvent(EventItem item);
    void UpdateEvent(EventItem item);
    void DeleteEvent(Guid id, int userId);
}
