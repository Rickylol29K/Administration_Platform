using AdministrationPlat.Models;
using AdministrationPlat.Pages.Teacher;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class GradingPageTests
{
    [TestMethod]
    public void OnGet_WithoutSession_Redirects()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Grading(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
    }

    [TestMethod]
    public void OnPostLoad_WithValidInput_LoadsSheet()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);
        var page = new Grading(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = cls.Id;
        page.AssessmentName = "Quiz";

        var result = page.OnPostLoad();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.SheetLoaded);
        Assert.AreEqual(1, page.StudentGrades.Count);
    }

    [TestMethod]
    public void OnPostSave_WithMissingClass_AddsModelError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Grading(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = 0;
        page.AssessmentName = "Quiz";

        var result = page.OnPostSave();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ContainsKey(nameof(page.SelectedClassId)));
    }

    [TestMethod]
    public void OnPostSave_WithValidData_SavesGrades()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);
        var page = new Grading(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = cls.Id;
        page.AssessmentName = "Quiz";
        page.StudentGrades = new List<StudentGradeEntry>
        {
            new() { StudentId = student.Id, Score = 9m, Comment = "Great" }
        };

        var result = page.OnPostSave();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, repository.GradeRecords.Count);
        Assert.AreEqual("Grades saved.", page.TempData["GradingMessage"]);
    }
}
