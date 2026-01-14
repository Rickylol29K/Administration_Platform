using System;
using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Logic.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting;

[TestClass]
public class AttendanceTests
{
    [TestMethod]
    public void SaveAttendance_SavesRecords()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveAttendance(cls.Id, DateTime.Today, new[]
        {
            new StudentAttendance { StudentId = student.Id, IsPresent = true }
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.AttendanceRecords.Count);
    }

    [TestMethod]
    public void SaveAttendance_WithNoStudents_StillSucceeds()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveAttendance(cls.Id, DateTime.Today, Array.Empty<StudentAttendance>());

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, repository.AttendanceRecords.Count);
    }
}
