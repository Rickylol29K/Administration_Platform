using AdministrationPlat.Pages.Shared.Teacher;
using AdministrationPlat.Models;
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
    public void OnGet_WithSession_LoadsDashboardData()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        repository.GradeRecords.Add(new GradeRecord
        {
            Id = 1,
            StudentId = student.Id,
            SchoolClassId = cls.Id,
            Assessment = "Quiz",
            DateRecorded = DateTime.Today
        });
        repository.Events.Add(new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event",
            Day = DateTime.Today.Day,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            UserId = 1
        });
        var logic = new ApplicationLogic(repository);
        var page = new TeacherIndex(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, page.ClassCount);
        Assert.AreEqual(1, page.StudentCount);
        Assert.AreEqual(1, page.UpcomingEvents.Count);
        Assert.AreEqual(1, page.RecentGrades.Count);
    }
}
