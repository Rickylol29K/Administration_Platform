using System;
using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Logic.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting;

[TestClass]
public class GradingTests
{
    [TestMethod]
    public void SaveGrades_SavesRecords()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveGrades(cls.Id, "Quiz 1", DateTime.Today, null, new[]
        {
            new StudentGradeEntry { StudentId = student.Id, Score = 9m, Comment = "Great" }
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.GradeRecords.Count);
    }

    [TestMethod]
    public void BuildGradeSheet_ForClass_ReturnsEntries()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.BuildGradeSheet(cls.Id, "Quiz 1", DateTime.Today);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value?.Entries.Count);
    }
}
