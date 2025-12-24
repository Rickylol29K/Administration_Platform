using AdministrationPlat.Models;
using DAL;
using Logic;

namespace AdministrationPlatformTesting;

[TestClass]
public class CalendarTests
{
    [TestMethod]
    public void HasCorrectDays()
    {
        var logic = new ApplicationLogic(new StubRepository());

        var calendar = logic.BuildCurrentMonth();
        var today = DateTime.Today;
        var expectedDays = DateTime.DaysInMonth(today.Year, today.Month);

        Assert.AreEqual(today.Year, calendar.Year);
        Assert.AreEqual(today.Month, calendar.Month);
        Assert.AreEqual(expectedDays, calendar.Days.Count);
        Assert.AreEqual(1, calendar.Days.First());
        Assert.AreEqual(expectedDays, calendar.Days.Last());
    }

    [TestMethod]
    public void MatchesTodayName()
    {
        var logic = new ApplicationLogic(new StubRepository());

        var calendar = logic.BuildCurrentMonth();
        var expectedName = DateTime.Today.ToString("MMMM");

        Assert.AreEqual(expectedName, calendar.MonthName);
    }

    [TestMethod]
    public void ReturnsAllMonthEvents()
    {
        var repo = new StubRepository
        {
            MonthEvents = new List<EventItem>
            {
                new() { Title = "One", Day = 1, Month = DateTime.Today.Month, Year = DateTime.Today.Year },
                new() { Title = "Two", Day = 2, Month = DateTime.Today.Month, Year = DateTime.Today.Year }
            }
        };

        var logic = new ApplicationLogic(repo);

        var view = logic.BuildCalendarView(userId: 1, selectedDay: 1);

        CollectionAssert.AreEqual(repo.MonthEvents, view.MonthEvents);
    }

    [TestMethod]
    public void FiltersBySelectedDay()
    {
        var today = DateTime.Today;
        var repo = new StubRepository
        {
            MonthEvents = new List<EventItem>
            {
                new() { Title = "Keep", Day = 3, Month = today.Month, Year = today.Year },
                new() { Title = "Skip", Day = 4, Month = today.Month, Year = today.Year }
            }
        };

        var logic = new ApplicationLogic(repo);

        var view = logic.BuildCalendarView(userId: 2, selectedDay: 3);

        Assert.AreEqual(1, view.SelectedDayEvents.Count);
        Assert.AreEqual("Keep", view.SelectedDayEvents[0].Title);
    }

    [TestMethod]
    public void SortsSelectedDayByTime()
    {
        var today = DateTime.Today;
        var repo = new StubRepository
        {
            MonthEvents = new List<EventItem>
            {
                new() { Title = "Later", Day = 5, Month = today.Month, Year = today.Year, Time = "15:00" },
                new() { Title = "Early", Day = 5, Month = today.Month, Year = today.Year, Time = "09:00" }
            }
        };

        var logic = new ApplicationLogic(repo);

        var view = logic.BuildCalendarView(userId: 3, selectedDay: 5);

        Assert.AreEqual("Early", view.SelectedDayEvents.First().Title);
        Assert.AreEqual("Later", view.SelectedDayEvents.Last().Title);
    }

    [TestMethod]
    public void ReturnsEmptyWhenNoEvents()
    {
        var today = DateTime.Today;
        var repo = new StubRepository
        {
            MonthEvents = new List<EventItem>
            {
                new() { Title = "Other Day", Day = today.Day + 1, Month = today.Month, Year = today.Year }
            }
        };

        var logic = new ApplicationLogic(repo);

        var view = logic.BuildCalendarView(userId: 4, selectedDay: today.Day);

        Assert.AreEqual(0, view.SelectedDayEvents.Count);
    }

    private class StubRepository : IDataRepository
    {
        public List<EventItem> MonthEvents { get; set; } = new();

        public List<EventItem> GetEventsForMonth(int userId, int year, int month) => MonthEvents;
        public EventItem? GetEvent(Guid id, int userId) => throw new NotImplementedException();
        public void AddEvent(EventItem item) => throw new NotImplementedException();
        public void UpdateEvent(EventItem item) => throw new NotImplementedException();
        public void DeleteEvent(Guid id, int userId) => throw new NotImplementedException();
        public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take) => throw new NotImplementedException();
        public User? GetUser(string username, string password) => throw new NotImplementedException();
        public User? GetUserById(int id) => throw new NotImplementedException();
        public bool UsernameExists(string username) => throw new NotImplementedException();
        public User CreateUser(string username, string password, bool isAdmin) => throw new NotImplementedException();
        public List<User> GetTeachers() => throw new NotImplementedException();
        public List<SchoolClass> GetClassesForTeacher(int teacherId) => throw new NotImplementedException();
        public List<SchoolClass> GetAllClasses() => throw new NotImplementedException();
        public SchoolClass AddClass(SchoolClass schoolClass) => throw new NotImplementedException();
        public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null) => throw new NotImplementedException();
        public string? GetClassName(int classId) => throw new NotImplementedException();
        public Student? GetStudentByEmail(string email) => throw new NotImplementedException();
        public Student AddStudent(Student student) => throw new NotImplementedException();
        public bool EnrollmentExists(int studentId, int classId) => throw new NotImplementedException();
        public void AddEnrollment(int studentId, int classId) => throw new NotImplementedException();
        public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId) => throw new NotImplementedException();
        public void RemoveEnrollment(int enrollmentId) => throw new NotImplementedException();
        public List<Student> GetStudentsForClass(int classId) => throw new NotImplementedException();
        public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date) => throw new NotImplementedException();
        public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records) => throw new NotImplementedException();
        public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date) => throw new NotImplementedException();
        public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records) => throw new NotImplementedException();
        public List<GradeRecord> GetRecentGrades(int teacherId, int take) => throw new NotImplementedException();
        public int GetClassCount(int teacherId) => throw new NotImplementedException();
        public int GetDistinctStudentCount(int teacherId) => throw new NotImplementedException();
        public List<Announcement> GetAnnouncements(int take) => throw new NotImplementedException();
        public List<Announcement> GetAllAnnouncements() => throw new NotImplementedException();
        public Announcement? GetAnnouncement(Guid id) => throw new NotImplementedException();
        public Announcement AddAnnouncement(Announcement announcement) => throw new NotImplementedException();
        public void DeleteAnnouncement(Guid id) => throw new NotImplementedException();
    }
}
