using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<Announcement> GetAnnouncements(int take) => _announcements.GetAnnouncements(take);

    public List<Announcement> GetAllAnnouncements() => _announcements.GetAllAnnouncements();

    public OperationResult<Announcement> CreateAnnouncement(int createdByUserId, string title, string? body) =>
        _announcements.CreateAnnouncement(createdByUserId, title, body);

    public OperationResult<bool> DeleteAnnouncement(Guid id) => _announcements.DeleteAnnouncement(id);
}
