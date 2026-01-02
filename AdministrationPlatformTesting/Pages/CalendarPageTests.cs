using AdministrationPlat.Models;
using AdministrationPlat.Pages.Teacher;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class CalendarPageTests
{
    [TestMethod]
    public void OnGet_WithoutSession_Redirects()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new CalendarModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
    }

    [TestMethod]
    public void OnPostAddEvent_WithEmptyTitle_ShowsError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new CalendarModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.NewEvent = new EventItem { Title = " " };

        var result = page.OnPostAddEvent(DateTime.Today.Day);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ContainsKey("NewEvent.Title"));
        Assert.IsTrue(page.ShowOverlay);
    }

    [TestMethod]
    public void OnPostUpdateEvent_WithInvalidTitle_ShowsError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new CalendarModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.NewEvent = new EventItem { Id = Guid.NewGuid(), Title = " " };

        var result = page.OnPostUpdateEvent(DateTime.Today.Day);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ContainsKey("NewEvent.Title"));
        Assert.IsTrue(page.ShowOverlay);
    }

    [TestMethod]
    public void OnPostEditEvent_LoadsEventIntoForm()
    {
        var repository = new FakeDataRepository();
        var ev = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event",
            Day = DateTime.Today.Day,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            UserId = 1
        };
        repository.Events.Add(ev);
        var logic = new ApplicationLogic(repository);
        var page = new CalendarModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnPostEditEvent(ev.Id, ev.Day);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(ev.Id, page.EditingId);
        Assert.AreEqual(ev.Title, page.NewEvent.Title);
        Assert.IsTrue(page.ShowOverlay);
    }

    [TestMethod]
    public void OnPostHideOverlay_ClearsFlag()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new CalendarModel(logic) { ShowOverlay = true };
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnPostHideOverlay();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsFalse(page.ShowOverlay);
    }
}
