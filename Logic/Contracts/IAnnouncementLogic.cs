using AdministrationPlat.Models;
using Logic.Models;

namespace Logic.Contracts;

public interface IAnnouncementLogic
{
    List<Announcement> GetAnnouncements(int take);
    List<Announcement> GetAllAnnouncements();
    OperationResult<Announcement> CreateAnnouncement(int createdByUserId, string title, string? body);
    OperationResult<bool> DeleteAnnouncement(Guid id);
}
