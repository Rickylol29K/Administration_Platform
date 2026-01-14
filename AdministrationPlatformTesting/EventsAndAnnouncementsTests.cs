using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting;

[TestClass]
public class EventsAndAnnouncementsTests
{
    [TestMethod]
    public void CreateEvent_WithBlankTitle_Fails()
    {
        var logic = new ApplicationLogic(new FakeDataRepository());

        var result = logic.CreateEvent(1, new EventItem
        {
            Title = " ",
            Day = 1,
            Month = 1,
            Year = 2025
        });

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void CreateEvent_WithTitle_Succeeds()
    {
        var logic = new ApplicationLogic(new FakeDataRepository());

        var result = logic.CreateEvent(1, new EventItem
        {
            Title = "Parent Meeting",
            Day = 1,
            Month = 1,
            Year = 2025
        });

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void CreateAnnouncement_WithBlankTitle_Fails()
    {
        var logic = new ApplicationLogic(new FakeDataRepository());

        var result = logic.CreateAnnouncement(1, " ", null);

        Assert.IsFalse(result.Success);
    }
}
