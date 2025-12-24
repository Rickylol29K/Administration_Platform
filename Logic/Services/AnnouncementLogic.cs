using AdministrationPlat.Models;
using DAL;
using Logic.Contracts;
using Logic.Models;

namespace Logic.Services;

internal sealed class AnnouncementLogic : IAnnouncementLogic
{
    private readonly IDataRepository _repository;

    public AnnouncementLogic(IDataRepository repository)
    {
        _repository = repository;
    }

    public List<Announcement> GetAnnouncements(int take) => _repository.GetAnnouncements(take);

    public List<Announcement> GetAllAnnouncements() => _repository.GetAllAnnouncements();

    public OperationResult<Announcement> CreateAnnouncement(int createdByUserId, string title, string? body)
    {
        var trimmedTitle = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            return OperationResult<Announcement>.Fail("Announcement title is required.");
        }

        var announcement = new Announcement
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

        var existing = _repository.GetAnnouncement(id);
        if (existing == null)
        {
            return OperationResult<bool>.Fail("Announcement not found.");
        }

        _repository.DeleteAnnouncement(id);
        return OperationResult<bool>.Ok(true);
    }
}
