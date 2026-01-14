using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting;

[TestClass]
public class AuthTests
{
    [TestMethod]
    public void Login_WithValidUser_Succeeds()
    {
        var repository = new FakeDataRepository();
        repository.CreateUser("teacher", "secret", false);
        var logic = new ApplicationLogic(repository);

        var result = logic.Login("teacher", "secret");

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Login_WithMissingFields_Fails()
    {
        var logic = new ApplicationLogic(new FakeDataRepository());

        var result = logic.Login(" ", "");

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void Register_WithDuplicateUsername_Fails()
    {
        var repository = new FakeDataRepository();
        repository.CreateUser("teacher", "secret", false);
        var logic = new ApplicationLogic(repository);

        var result = logic.Register("teacher", "newpass", false);

        Assert.IsFalse(result.Success);
    }
}
