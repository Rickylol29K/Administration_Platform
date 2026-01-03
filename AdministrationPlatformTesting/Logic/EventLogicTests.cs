using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class EventLogicTests
{
    [TestMethod]
    public void CreateEvent_WithEmptyTitle_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.CreateEvent(1, new EventItem { Title = "  " });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Event title is required.", result.Error);
    }

    [TestMethod]
    public void BuildCalendarView_FiltersSelectedDayEvents()
    {
        var repository = new FakeDataRepository();
        var today = DateTime.Today;
        repository.Events.Add(new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Keep",
            Day = 5,
            Month = today.Month,
            Year = today.Year,
            UserId = 1
        });
        repository.Events.Add(new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Skip",
            Day = 7,
            Month = today.Month,
            Year = today.Year,
            UserId = 1
        });
        var logic = new ApplicationLogic(repository);

        var view = logic.BuildCalendarView(1, 5);

        Assert.AreEqual(1, view.SelectedDayEvents.Count);
        Assert.AreEqual("Keep", view.SelectedDayEvents[0].Title);
    }
}
