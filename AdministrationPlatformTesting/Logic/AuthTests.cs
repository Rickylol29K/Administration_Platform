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
    public void Login_WithValidCredentials_ReturnsUser()
    {
        var repository = new FakeDataRepository();
        var user = repository.CreateUser("teacher", "secret");
        var logic = new ApplicationLogic(repository);

        var result = logic.Login("teacher", "secret");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(user.Id, result.Value.Id);
    }

    [TestMethod]
    public void Login_WithInvalidCredentials_Fails()
    {
        var repository = new FakeDataRepository();
        repository.CreateUser("teacher", "secret");
        var logic = new ApplicationLogic(repository);

        var result = logic.Login("teacher", "wrong");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Invalid username or password.", result.Error);
    }

    [TestMethod]
    public void Register_WithMissingFields_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.Register("", " ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Choose a username and password.", result.Error);
    }

    [TestMethod]
    public void Register_WithDuplicateUsername_Fails()
    {
        var repository = new FakeDataRepository();
        repository.CreateUser("teacher", "secret");
        var logic = new ApplicationLogic(repository);

        var result = logic.Register("teacher", "newpass");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Username already exists.", result.Error);
    }

    [TestMethod]
    public void Register_WithValidData_CreatesUser()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.Register("teacher", "secret");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("teacher", result.Value.Username);
        Assert.AreEqual(1, repository.Users.Count);
    }
}
