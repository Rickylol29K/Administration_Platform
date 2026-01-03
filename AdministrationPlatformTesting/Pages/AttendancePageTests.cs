using AdministrationPlat.Models;
using AdministrationPlat.Pages.Shared.Teacher;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Pages;

[TestClass]
public class AttendancePageTests
{
    [TestMethod]
    public void OnPostSave_WithMissingClass_ShowsError()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);
        var page = new Attendance(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = 0;

        var result = page.OnPostSave();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsTrue(page.ModelState.ContainsKey(nameof(page.SelectedClassId)));
    }

    [TestMethod]
    public void OnPostSave_WithValidData_Saves()
    {
        var repository = new FakeDataRepository();
        var cls = repository.AddClass(new SchoolClass { Name = "Math", TeacherId = 1 });
        var student = repository.AddStudent(new Student { FirstName = "Sam", LastName = "Jones" });
        repository.AddEnrollment(student.Id, cls.Id);
        var logic = new ApplicationLogic(repository);
        var page = new Attendance(logic);
        var session = new TestSession();
        var context = PageModelTestHelper.CreateHttpContextWithSession(session, 1);
        PageModelTestHelper.AttachPageContext(page, context);

        page.SelectedClassId = cls.Id;
        page.StudentAttendances = new List<StudentAttendance>
        {
            new() { StudentId = student.Id, IsPresent = true }
        };

        var result = page.OnPostSave();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(1, repository.AttendanceRecords.Count);
    }
}
