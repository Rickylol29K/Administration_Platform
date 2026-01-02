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
        Assert.AreEqual(0, repository.Classes.Count);
    }

    [TestMethod]
    public void CreateClass_TrimsAndNormalizesFields()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.CreateClass(3, "  Algebra  ", "  ", "  Notes ");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("Algebra", result.Value.Name);
        Assert.IsNull(result.Value.Room);
        Assert.AreEqual("Notes", result.Value.Description);
        Assert.AreEqual(3, result.Value.TeacherId);
    }

    [TestMethod]
    public void LoadClassOverlay_WithMissingClass_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.LoadClassOverlay(999, 1);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Unable to load the requested class.", result.Error);
    }

    [TestMethod]
    public void LoadClassOverlay_OrdersEnrollmentsByName()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var studentB = repository.AddStudent(new Student { FirstName = "Amy", LastName = "Brown" });
        var studentA = repository.AddStudent(new Student { FirstName = "Zed", LastName = "Adams" });
        repository.AddEnrollment(studentB.Id, classInfo.Id);
        repository.AddEnrollment(studentA.Id, classInfo.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.LoadClassOverlay(classInfo.Id, 1);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("Adams", result.Value.Enrollments[0].Student?.LastName);
        Assert.AreEqual("Brown", result.Value.Enrollments[1].Student?.LastName);
    }

    [TestMethod]
    public void AddStudentToClass_WithMissingNames_Fails()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);

        var result = logic.AddStudentToClass(1, classInfo.Id, "", " ", "test@example.com");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Student first and last name are required.", result.Message);
    }

    [TestMethod]
    public void AddStudentToClass_UsesExistingStudentByEmail()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone", Email = "a@b.com" });
        var logic = new ApplicationLogic(repository);

        var result = logic.AddStudentToClass(1, classInfo.Id, "Alex", "Stone", "a@b.com");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.Students.Count);
        Assert.IsTrue(repository.EnrollmentExists(student.Id, classInfo.Id));
    }

    [TestMethod]
    public void AddStudentToClass_AddsNewStudentAndEnrollment()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);

        var result = logic.AddStudentToClass(1, classInfo.Id, "Alex", "Stone", "a@b.com");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.Students.Count);
        Assert.AreEqual(1, repository.Enrollments.Count);
    }

    [TestMethod]
    public void AddStudentToClass_WhenAlreadyEnrolled_ReturnsAlreadyEnrolled()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone", Email = "a@b.com" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        var logic = new ApplicationLogic(repository);

        var result = logic.AddStudentToClass(1, classInfo.Id, "Alex", "Stone", "a@b.com");

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.AlreadyEnrolled);
    }

    [TestMethod]
    public void RemoveStudentFromClass_WhenMissingEnrollment_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.RemoveStudentFromClass(1, 999);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Enrollment not found.", result.Message);
    }

    [TestMethod]
    public void RemoveStudentFromClass_RemovesEnrollment()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        var enrollment = repository.Enrollments.First();
        var logic = new ApplicationLogic(repository);

        var result = logic.RemoveStudentFromClass(1, enrollment.Id);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, repository.Enrollments.Count);
    }

    [TestMethod]
    public void GetCounts_ReturnsClassAndStudentTotals()
    {
        var repository = new FakeDataRepository();
        var classInfo = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 7 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone" });
        repository.AddEnrollment(student.Id, classInfo.Id);
        var logic = new ApplicationLogic(repository);

        Assert.AreEqual(1, logic.GetClassCount(7));
        Assert.AreEqual(1, logic.GetDistinctStudentCount(7));
    }
}
