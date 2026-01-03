using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class AuthTests
{
    [TestMethod]
    public void Login_WithEmptyFields_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.Login(" ", "");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Enter both username and password.", result.Error);
    }

    [TestMethod]
    public void Register_WithDuplicateUsername_Fails()
    {
        var repository = new FakeDataRepository();
        repository.CreateUser("teacher", "secret", false);
        var logic = new ApplicationLogic(repository);

        var result = logic.Register("teacher", "newpass", false);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Username already exists.", result.Error);
    }

    [TestMethod]
    public void Register_WithAdminFlag_SetsAdmin()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.Register("admin", "secret", true);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.IsTrue(result.Value.IsAdmin);
    }
}
