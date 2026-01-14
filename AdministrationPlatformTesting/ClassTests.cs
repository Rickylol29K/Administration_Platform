using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting;

[TestClass]
public class ClassTests
{
    [TestMethod]
    public void CreateClass_WithName_Succeeds()
    {
        var logic = new ApplicationLogic(new FakeDataRepository());

        var result = logic.CreateClass(1, "Math 101", "Room 1", "Basics");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void CreateClass_WithBlankName_Fails()
    {
        var logic = new ApplicationLogic(new FakeDataRepository());

        var result = logic.CreateClass(1, " ", null, null);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void AddStudentToClassAsAdmin_AddsEnrollment()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);

        var result = logic.AddStudentToClassAsAdmin(cls.Id, "Sam", "Jones", "sam@test.local");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.Enrollments.Count);
    }
}
