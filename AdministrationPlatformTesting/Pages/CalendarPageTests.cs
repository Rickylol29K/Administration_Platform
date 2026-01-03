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
    public void OnPostAddEvent_WithEmptyTitle_ShowsError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new CalendarModel(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.NewEvent = new EventItem { Title = " " };

        var today = DateTime.Today;
        var result = page.OnPostAddEvent(today.Day, today.Month, today.Year);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ContainsKey("NewEvent.Title"));
    }
}
