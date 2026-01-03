using AdministrationPlat.Models;
using AdministrationPlatformTesting.Infrastructure;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class AnnouncementLogicTests
{
    [TestMethod]
    public void CreateAnnouncement_WithEmptyTitle_Fails()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.CreateAnnouncement(1, " ", "Body");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Announcement title is required.", result.Error);
    }

    [TestMethod]
    public void CreateAnnouncement_WithValidData_Saves()
    {
        var repository = new FakeDataRepository();
        var logic = new ApplicationLogic(repository);

        var result = logic.CreateAnnouncement(2, "Update", "Hello");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, repository.Announcements.Count);
        Assert.AreEqual(2, repository.Announcements[0].CreatedByUserId);
    }

    [TestMethod]
    public void DeleteAnnouncement_RemovesExisting()
    {
        var repository = new FakeDataRepository();
        var announcement = new Announcement { Id = Guid.NewGuid(), Title = "Update" };
        repository.Announcements.Add(announcement);
        var logic = new ApplicationLogic(repository);

        var result = logic.DeleteAnnouncement(announcement.Id);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, repository.Announcements.Count);
    }
}
