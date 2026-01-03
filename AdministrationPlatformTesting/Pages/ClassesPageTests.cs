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
    public void OnPostAddClass_ForTeacher_ShowsAdminOnlyError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Classes(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        var result = page.OnPostAddClass();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, page.ModelState.ErrorCount);
    }

    [TestMethod]
    public void OnPostAddStudent_ForTeacher_ShowsAdminOnlyError()
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
        Assert.AreEqual(1, page.ModelState.ErrorCount);
        Assert.AreEqual(0, repository.Enrollments.Count);
    }
}
