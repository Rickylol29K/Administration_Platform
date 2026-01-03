using AdministrationPlat.Models;
using AdministrationPlat.Pages.Shared.Teacher;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class TeacherIndexPageTests
{
    [TestMethod]
    public void OnGet_WithoutSession_Redirects()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new TeacherIndex(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
    }

    [TestMethod]
    public void OnGet_WithSession_LoadsAnnouncements()
    {
        var repository = new FakeDataRepository();
        repository.Announcements.Add(new Announcement { Id = Guid.NewGuid(), Title = "Update" });
        var logic = new ApplicationLogic(repository);
        var page = new TeacherIndex(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, page.Announcements.Count);
    }
}
