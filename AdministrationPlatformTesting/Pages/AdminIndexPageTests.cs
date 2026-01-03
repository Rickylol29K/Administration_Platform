using AdministrationPlat.Pages.Admin;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class AdminIndexPageTests
{
    [TestMethod]
    public void OnGet_WithoutAdmin_Redirects()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new AdminIndex(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1, isAdmin: false);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
    }

    [TestMethod]
    public void OnPostAddAnnouncement_WithValidData_Publishes()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new AdminIndex(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 10, isAdmin: true);
        PageModelTestHelper.AttachPageContext(page, context);

        page.AnnouncementTitle = "Update";
        page.AnnouncementBody = "Hello";

        var result = page.OnPostAddAnnouncement();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, repository.Announcements.Count);
    }
}
