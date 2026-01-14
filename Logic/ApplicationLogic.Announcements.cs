using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<Announcement> GetAnnouncements(int take)
    {
        return _repository.GetAnnouncements(take);
    }

    public List<Announcement> GetAllAnnouncements()
    {
        return _repository.GetAllAnnouncements();
    }

    public OperationResult<Announcement> CreateAnnouncement(int createdByUserId, string title, string? body)
    {
        string trimmedTitle = (title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            return OperationResult<Announcement>.Fail("Announcement title is required.");
        }

        Announcement announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = trimmedTitle,
            Body = string.IsNullOrWhiteSpace(body) ? null : body.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _repository.AddAnnouncement(announcement);
        return OperationResult<Announcement>.Ok(announcement);
    }

    public OperationResult<bool> DeleteAnnouncement(Guid id)
    {
        if (id == Guid.Empty)
        {
            return OperationResult<bool>.Fail("Announcement not found.");
        }

        Announcement? existing = _repository.GetAnnouncement(id);
        if (existing == null)
        {
            return OperationResult<bool>.Fail("Announcement not found.");
        }

        _repository.DeleteAnnouncement(id);
        return OperationResult<bool>.Ok(true);
    }
}
