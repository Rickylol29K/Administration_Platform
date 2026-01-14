using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public interface ILogicService
{
    OperationResult<User> Login(string username, string password);
    OperationResult<User> Register(string username, string password, bool isAdmin);
    User? GetUser(string username, string password);
    User? GetUserById(int id);
    bool UsernameExists(string username);
    User CreateUser(string username, string password, bool isAdmin);
    List<User> GetTeachers();

    List<SchoolClass> GetClassesForTeacher(int teacherId);
    List<SchoolClass> GetAllClasses();
    SchoolClass AddClass(SchoolClass schoolClass);
    SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null);
    string? GetClassName(int classId);
    OperationResult<SchoolClass> CreateClass(int teacherId, string name, string? room, string? description);
    OperationResult<ClassOverlay> LoadClassOverlay(int classId, int? teacherId = null);
    ClassMembershipResult AddStudentToClass(int teacherId, int classId, string firstName, string lastName, string? email);
    ClassMembershipResult AddStudentToClassAsAdmin(int classId, string firstName, string lastName, string? email);
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

    List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date);
    void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records);
    List<SchoolClass> GetClassesForUserOrFallback(int userId);
    OperationResult<AttendanceRoster> BuildAttendanceRoster(int classId, DateTime date);
    OperationResult<AttendanceRoster> SaveAttendance(int classId, DateTime date, IEnumerable<StudentAttendance> records);

    List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date);
    void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records);
    List<GradeRecord> GetRecentGrades(int teacherId, int take);
    OperationResult<GradeSheet> BuildGradeSheet(int classId, string assessment, DateTime date);
    OperationResult<GradeSheet> SaveGrades(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<StudentGradeEntry> entries);

    List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take);
    List<EventItem> GetEventsForMonth(int userId, int year, int month);
    EventItem? GetEvent(Guid id, int userId);
    void AddEvent(EventItem item);
    void UpdateEvent(EventItem item);
    void DeleteEvent(Guid id, int userId);
    OperationResult<EventItem> CreateEvent(int userId, EventItem item);
    OperationResult<EventItem> UpdateEventDetails(int userId, EventItem item);
    OperationResult<bool> DeleteEventForUser(int userId, Guid id);
    CalendarData BuildCurrentMonth();
    CalendarView BuildCalendarView(int userId, int selectedDay);
    CalendarView BuildCalendarView(int userId, int year, int month, int selectedDay);

    List<Announcement> GetAnnouncements(int take);
    List<Announcement> GetAllAnnouncements();
    OperationResult<Announcement> CreateAnnouncement(int createdByUserId, string title, string? body);
    OperationResult<bool> DeleteAnnouncement(Guid id);
}
