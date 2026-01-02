using AdministrationPlat.Pages;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class IndexPageTests
{
    [TestMethod]
    public void OnPostLogin_WithValidUser_RedirectsAndSetsSession()
    {
        var repository = new FakeDataRepository();
        var user = repository.CreateUser("teacher", "secret");
        var logic = new ApplicationLogic(repository);
        var page = new IndexModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        page.LoginUsername = "teacher";
        page.LoginPassword = "secret";

        var result = page.OnPostLogin();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        Assert.AreEqual(user.Id, context.Session.GetInt32("UserId"));
        Assert.AreEqual("teacher", context.Session.GetString("Username"));
    }

    [TestMethod]
    public void OnPostLogin_WithInvalidUser_ShowsError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new IndexModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        page.LoginUsername = "teacher";
        page.LoginPassword = "secret";

        var result = page.OnPostLogin();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual("Invalid username or password.", page.Message);
        Assert.AreEqual("validation-summary", page.MessageCssClass);
    }

    [TestMethod]
    public void OnPostRegister_WithValidUser_ShowsSuccess()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new IndexModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        page.RegisterUsername = "teacher";
        page.RegisterPassword = "secret";

        var result = page.OnPostRegister();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual("Registration successful! You can now log in.", page.Message);
        Assert.AreEqual("status-message", page.MessageCssClass);
    }

    [TestMethod]
    public void OnPostRegister_WithDuplicateUser_ShowsError()
    {
        var repository = new FakeDataRepository();
        repository.CreateUser("teacher", "secret");
        var logic = new ApplicationLogic(repository);
        var page = new IndexModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        page.RegisterUsername = "teacher";
        page.RegisterPassword = "secret";

        var result = page.OnPostRegister();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual("Username already exists.", page.Message);
        Assert.AreEqual("validation-summary", page.MessageCssClass);
    }
}
