using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take)
    {
        return _repository.GetUpcomingEvents(userId, today, take);
    }

    public List<EventItem> GetEventsForMonth(int userId, int year, int month)
    {
        return _repository.GetEventsForMonth(userId, year, month);
    }

    public EventItem? GetEvent(Guid id, int userId)
    {
        return _repository.GetEvent(id, userId);
    }

    public void AddEvent(EventItem item)
    {
        _repository.AddEvent(item);
    }

    public void UpdateEvent(EventItem item)
    {
        _repository.UpdateEvent(item);
    }

    public void DeleteEvent(Guid id, int userId)
    {
        _repository.DeleteEvent(id, userId);
    }

    public OperationResult<EventItem> CreateEvent(int userId, EventItem item)
    {
        string title = (item.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return OperationResult<EventItem>.Fail("Event title is required.");
        }

        EventItem normalized = new EventItem
        {
            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
            Title = title,
            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
            Location = string.IsNullOrWhiteSpace(item.Location) ? null : item.Location.Trim(),
            Time = string.IsNullOrWhiteSpace(item.Time) ? null : item.Time.Trim(),
            Day = item.Day,
            Month = item.Month,
            Year = item.Year,
            UserId = userId
        };

        _repository.AddEvent(normalized);
        return OperationResult<EventItem>.Ok(normalized);
    }

    public OperationResult<EventItem> UpdateEventDetails(int userId, EventItem item)
    {
        EventItem? existing = _repository.GetEvent(item.Id, userId);
        if (existing == null)
        {
            return OperationResult<EventItem>.Fail("Event not found.");
        }

        string title = (item.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return OperationResult<EventItem>.Fail("Event title is required.");
        }

        existing.Title = title;
        existing.Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim();
        existing.Location = string.IsNullOrWhiteSpace(item.Location) ? null : item.Location.Trim();
        existing.Time = string.IsNullOrWhiteSpace(item.Time) ? null : item.Time.Trim();

        _repository.UpdateEvent(existing);
        return OperationResult<EventItem>.Ok(existing);
    }

    public OperationResult<bool> DeleteEventForUser(int userId, Guid id)
    {
        EventItem? existing = _repository.GetEvent(id, userId);
        if (existing == null)
        {
            return OperationResult<bool>.Fail("Event not found.");
        }

        _repository.DeleteEvent(id, userId);
        return OperationResult<bool>.Ok(true);
    }
}
