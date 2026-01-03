using AdministrationPlat.Models;
using Logic.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Models;

[TestClass]
public class ModelDefaultsTests
{
    [TestMethod]
    public void Model_Collections_AreInitialized()
    {
        var student = new Student();
        var schoolClass = new SchoolClass();
        var gradeSheet = new GradeSheet();
        var attendanceRoster = new AttendanceRoster();
        var classOverlay = new ClassOverlay();
        var calendarView = new CalendarView();

        Assert.IsNotNull(student.Enrollments);
        Assert.IsNotNull(student.AttendanceRecords);
        Assert.IsNotNull(student.GradeRecords);
        Assert.IsNotNull(schoolClass.Enrollments);
        Assert.IsNotNull(gradeSheet.Entries);
        Assert.IsNotNull(attendanceRoster.Students);
        Assert.IsNotNull(classOverlay.Enrollments);
        Assert.IsNotNull(calendarView.MonthEvents);
        Assert.IsNotNull(calendarView.SelectedDayEvents);
    }

    [TestMethod]
    public void OperationResult_FactoryMethods_SetState()
    {
        var ok = OperationResult<string>.Ok("value");
        var fail = OperationResult<string>.Fail("error");

        Assert.IsTrue(ok.Success);
        Assert.AreEqual("value", ok.Value);
        Assert.IsFalse(fail.Success);
        Assert.AreEqual("error", fail.Error);
    }
}
