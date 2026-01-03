using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Logic.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class AttendanceLogicTests
{
    [TestMethod]
    public void BuildAttendanceRoster_MapsExistingAttendance()
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
        var logic = new ApplicationLogic(repository);

        var roster = logic.BuildAttendanceRoster(cls.Id, DateTime.Today);

        Assert.IsTrue(roster.Success);
        Assert.AreEqual(1, roster.Value?.Students.Count);
        Assert.IsTrue(roster.Value?.Students[0].IsPresent ?? false);
    }

    [TestMethod]
    public void SaveAttendance_PersistsRecords()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveAttendance(cls.Id, DateTime.Today, new List<StudentAttendance>
        {
            new() { StudentId = student.Id, IsPresent = true }
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.AttendanceRecords.Count);
    }
}
