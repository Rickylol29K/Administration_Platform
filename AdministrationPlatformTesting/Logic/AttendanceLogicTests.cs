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
    public void GetClassesForUserOrFallback_ReturnsTeacherClasses()
    {
        var repository = new FakeDataRepository();
        repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        repository.AddClass(new SchoolClass { Name = "Science", TeacherId = 2 });
        var logic = new ApplicationLogic(repository);

        var result = logic.GetClassesForUserOrFallback(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Math", result[0].Name);
    }

    [TestMethod]
    public void GetClassesForUserOrFallback_UsesAllClassesWhenNoneOwned()
    {
        var repository = new FakeDataRepository();
        repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        repository.AddClass(new SchoolClass { Name = "Science", TeacherId = 2 });
        var logic = new ApplicationLogic(repository);

        var result = logic.GetClassesForUserOrFallback(3);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void BuildAttendanceRoster_MapsExistingAttendance()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        repository.AttendanceRecords.Add(new AttendanceRecord
        {
            Id = 1,
            StudentId = student.Id,
            SchoolClassId = classInfo.Id,
            Date = DateTime.Today,
            IsPresent = true
        });
        var logic = new ApplicationLogic(repository);

        var roster = logic.BuildAttendanceRoster(classInfo.Id, DateTime.Today);

        Assert.IsTrue(roster.Success);
        Assert.AreEqual("Math", roster.Value?.ClassName);
        Assert.AreEqual(1, roster.Value?.Students.Count);
        Assert.IsTrue(roster.Value?.Students[0].IsPresent ?? false);
    }

    [TestMethod]
    public void SaveAttendance_PersistsAndReturnsRoster()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveAttendance(classInfo.Id, DateTime.Today, new List<StudentAttendance>
        {
            new() { StudentId = student.Id, IsPresent = true }
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.AttendanceRecords.Count);
        Assert.IsTrue(result.Value?.Students[0].IsPresent ?? false);
    }
}
