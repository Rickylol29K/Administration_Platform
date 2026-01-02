using AdministrationPlat.Models;
using AdministrationPlat.Pages.Teacher;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class ClassesPageTests
{
    [TestMethod]
    public void OnGet_WithoutSession_Redirects()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnGet();

        Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
    }

    [TestMethod]
    public void OnPostAddClass_WithMissingName_AddsModelError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.NewClassName = " ";

        var result = page.OnPostAddClass();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ContainsKey(nameof(page.NewClassName)));
    }

    [TestMethod]
    public void OnPostAddClass_WithValidData_CreatesClass()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.NewClassName = "History";

        var result = page.OnPostAddClass();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, repository.Classes.Count);
        Assert.AreEqual("Class \"History\" created.", page.TempData["ClassMessage"]);
    }

    [TestMethod]
    public void OnPostShowOverlay_LoadsClassData()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnPostShowOverlay(cls.Id);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ShowOverlay);
        Assert.IsNotNull(page.ActiveClass);
        Assert.AreEqual(1, page.ActiveEnrollments.Count);
    }

    [TestMethod]
    public void OnPostAddStudent_WhenLogicFails_ShowsError()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = cls.Id;
        page.NewStudentFirstName = "";
        page.NewStudentLastName = "";

        var result = page.OnPostAddStudent();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ShowOverlay);
        Assert.AreEqual(1, page.ModelState.ErrorCount);
    }

    [TestMethod]
    public void OnPostAddStudent_WithValidData_AddsEnrollment()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = cls.Id;
        page.NewStudentFirstName = "Alex";
        page.NewStudentLastName = "Stone";

        var result = page.OnPostAddStudent();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, repository.Enrollments.Count);
        Assert.AreEqual("Alex Stone added to History.", page.TempData["ClassMessage"]);
    }

    [TestMethod]
    public void OnPostRemoveStudent_RemovesEnrollment()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Alex", LastName = "Stone" });
        repository.AddEnrollment(student.Id, cls.Id);
        var enrollment = repository.Enrollments[0];
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnPostRemoveStudent(enrollment.Id);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(0, repository.Enrollments.Count);
        Assert.IsTrue(page.ShowOverlay);
    }

    [TestMethod]
    public void OnPostHideOverlay_SetsFlagFalse()
    {
        var repository = new FakeDataRepository();
        repository.AddClass(new SchoolClass { Name = "History", TeacherId = 1 });
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic) { ShowOverlay = true };
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnPostHideOverlay();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsFalse(page.ShowOverlay);
    }
}
