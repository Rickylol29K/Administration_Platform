using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class ApplicationLogicDelegationTests
{
    [TestMethod]
    public void UserAndClassDelegates_ReturnExpectedValues()
    {
        var repository = new FakeDataRepository();
        var user = repository.CreateUser("teacher", "secret");
        var logic = new ApplicationLogic(repository);

        Assert.AreEqual(user, logic.GetUser("teacher", "secret"));
        Assert.IsTrue(logic.UsernameExists("teacher"));
        Assert.AreEqual("new", logic.CreateUser("new", "pw").Username);

        var createdClass = logic.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        Assert.AreEqual("Math", createdClass.Name);
        Assert.AreEqual(1, logic.GetClassesForTeacher(1).Count);
        Assert.AreEqual(1, logic.GetAllClasses().Count);
        Assert.AreEqual(createdClass.Name, logic.GetClassName(createdClass.Id));
    }

    [TestMethod]
    public void EnrollmentDelegates_WorkThroughRepository()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        var logic = new ApplicationLogic(repository);

        Assert.IsFalse(logic.EnrollmentExists(student.Id, cls.Id));
        logic.AddEnrollment(student.Id, cls.Id);
        Assert.IsTrue(logic.EnrollmentExists(student.Id, cls.Id));
        Assert.AreEqual(1, logic.GetStudentsForClass(cls.Id).Count);
    }

    [TestMethod]
    public void AttendanceAndGradeDelegates_ReturnData()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        repository.AttendanceRecords.Add(new AttendanceRecord
        {
            Id = 1,
            StudentId = student.Id,
            SchoolClassId = cls.Id,
            Date = DateTime.Today,
            IsPresent = true
        });
        repository.GradeRecords.Add(new GradeRecord
        {
            Id = 1,
            StudentId = student.Id,
            SchoolClassId = cls.Id,
            Assessment = "Quiz",
            DateRecorded = DateTime.Today,
            Score = 9m
        });
        var logic = new ApplicationLogic(repository);

        Assert.AreEqual(1, logic.GetAttendanceRecords(cls.Id, DateTime.Today).Count);
        Assert.AreEqual(1, logic.GetGradeRecords(cls.Id, "Quiz", DateTime.Today).Count);
    }

    [TestMethod]
    public void EventDelegates_ReturnData()
    {
        var repository = new FakeDataRepository();
        var ev = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event",
            Day = DateTime.Today.Day,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            UserId = 3
        };
        repository.Events.Add(ev);
        var logic = new ApplicationLogic(repository);

        Assert.IsNotNull(logic.GetEvent(ev.Id, 3));
        Assert.AreEqual(1, logic.GetEventsForMonth(3, ev.Year, ev.Month).Count);
        Assert.AreEqual(1, logic.GetUpcomingEvents(3, DateTime.Today, 5).Count);

        logic.DeleteEvent(ev.Id, 3);
        Assert.AreEqual(0, repository.Events.Count);
    }
}
