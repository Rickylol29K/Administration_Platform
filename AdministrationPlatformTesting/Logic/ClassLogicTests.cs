using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class ClassLogicTests
{
    [TestMethod]
    public void CreateClass_WithEmptyName_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.CreateClass(1, "  ", "Room", "Desc");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Class name is required.", result.Error);
    }

    [TestMethod]
    public void AddStudentToClassAsAdmin_AddsStudentAndEnrollment()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 3 });
        var logic = new ApplicationLogic(repository);

        var result = logic.AddStudentToClassAsAdmin(cls.Id, "Alex", "Stone", "a@b.com");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.Students.Count);
        Assert.AreEqual(1, repository.Enrollments.Count);
    }

    [TestMethod]
    public void RemoveStudentFromClass_RemovesEnrollment()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone" });
        repository.AddEnrollment(student.Id, cls.Id);
        var enrollment = repository.Enrollments[0];
        var logic = new ApplicationLogic(repository);

        var result = logic.RemoveStudentFromClass(1, enrollment.Id);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, repository.Enrollments.Count);
    }
}
