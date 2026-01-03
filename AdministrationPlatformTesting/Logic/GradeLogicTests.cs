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
    public void SaveGrades_PersistsScores()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.SaveGrades(cls.Id, "Quiz", DateTime.Today, 10m, new List<StudentGradeEntry>
        {
            new() { StudentId = student.Id, Score = 8m, Comment = "Solid" }
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.GradeRecords.Count);
    }
}
