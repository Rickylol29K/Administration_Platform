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
    public void CreateEvent_TrimsFieldsAndSetsUser()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.CreateEvent(5, new EventItem
        {
            Title = "  Meeting ",
            Description = "  Notes ",
            Location = " Room 1 ",
            Time = " 10:00 "
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.Events.Count);
        Assert.AreEqual(5, repository.Events[0].UserId);
        Assert.AreEqual("Meeting", repository.Events[0].Title);
        Assert.AreEqual("Notes", repository.Events[0].Description);
    }

    [TestMethod]
    public void UpdateEventDetails_WhenMissing_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.UpdateEventDetails(1, new EventItem { Id = Guid.NewGuid(), Title = "Event" });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Event not found.", result.Error);
    }

    [TestMethod]
    public void UpdateEventDetails_WithEmptyTitle_Fails()
    {
        var repository = new FakeDataRepository();
        var existing = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event",
            Day = 1,
            Month = 1,
            Year = 2024,
            UserId = 1
        };
        repository.Events.Add(existing);
        var logic = new ApplicationLogic(repository);

        var result = logic.UpdateEventDetails(1, new EventItem { Id = existing.Id, Title = " " });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Event title is required.", result.Error);
    }

    [TestMethod]
    public void UpdateEventDetails_UpdatesExistingEvent()
    {
        var repository = new FakeDataRepository();
        var existing = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event",
            Day = 1,
            Month = 1,
            Year = 2024,
            UserId = 1
        };
        repository.Events.Add(existing);
        var logic = new ApplicationLogic(repository);

        var result = logic.UpdateEventDetails(1, new EventItem
        {
            Id = existing.Id,
            Title = " Updated ",
            Description = " Notes "
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Updated", repository.Events[0].Title);
        Assert.AreEqual("Notes", repository.Events[0].Description);
    }

    [TestMethod]
    public void DeleteEventForUser_RemovesExistingEvent()
    {
        var repository = new FakeDataRepository();
        var existing = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event",
            Day = 1,
            Month = 1,
            Year = 2024,
            UserId = 1
        };
        repository.Events.Add(existing);
        var logic = new ApplicationLogic(repository);

        var result = logic.DeleteEventForUser(1, existing.Id);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, repository.Events.Count);
    }

    [TestMethod]
    public void BuildCurrentMonth_ReturnsDaysForTodayMonth()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var calendar = logic.BuildCurrentMonth();

        var expectedDays = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        Assert.AreEqual(expectedDays, calendar.Days.Count);
    }

    [TestMethod]
    public void BuildCalendarView_FiltersSelectedDayEvents()
    {
        var repository = new FakeDataRepository();
        var today = DateTime.Today;
        repository.Events.Add(new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event1",
            Day = 5,
            Month = today.Month,
            Year = today.Year,
            UserId = 1
        });
        repository.Events.Add(new EventItem
        {
            Id = Guid.NewGuid(),
            Title = "Event2",
            Day = 7,
            Month = today.Month,
            Year = today.Year,
            UserId = 1
        });
        var logic = new ApplicationLogic(repository);

        var view = logic.BuildCalendarView(1, 5);

        Assert.AreEqual(1, view.SelectedDayEvents.Count);
        Assert.AreEqual("Event1", view.SelectedDayEvents[0].Title);
    }
}
