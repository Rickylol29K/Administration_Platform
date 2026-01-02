using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Logic.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class GradeLogicTests
{
    [TestMethod]
    public void BuildGradeSheet_WhenClassMissing_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.BuildGradeSheet(99, "Quiz", DateTime.Today);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Class not found.", result.Error);
    }

    [TestMethod]
    public void BuildGradeSheet_MapsStudentsAndExistingGrades()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        repository.GradeRecords.Add(new GradeRecord
        {
            Id = 1,
            StudentId = student.Id,
            SchoolClassId = classInfo.Id,
            Assessment = "Quiz",
            DateRecorded = DateTime.Today,
            Score = 9.5m,
            MaxScore = 10m,
            Comments = "Nice work"
        });
        var logic = new ApplicationLogic(repository);

        var result = logic.BuildGradeSheet(classInfo.Id, "Quiz", DateTime.Today);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value?.Entries.Count);
        Assert.AreEqual(9.5m, result.Value?.Entries[0].Score);
        Assert.AreEqual("Nice work", result.Value?.Entries[0].Comment);
    }

    [TestMethod]
    public void SaveGrades_PersistsScoresAndReturnsSheet()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveGrades(classInfo.Id, "Quiz", DateTime.Today, 10m, new List<StudentGradeEntry>
        {
            new() { StudentId = student.Id, Score = 8m, Comment = "Solid" }
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.GradeRecords.Count);
        Assert.AreEqual(8m, repository.GradeRecords[0].Score);
    }
}
