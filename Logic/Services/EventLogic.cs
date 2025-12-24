using AdministrationPlat.Models;
using DAL;
using Logic.Contracts;
using Logic.Models;

namespace Logic.Services;

internal sealed class EventLogic : IEventLogic
{
    private readonly IDataRepository _repository;

    public EventLogic(IDataRepository repository)
    {
        _repository = repository;
    }

    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take) =>
        _repository.GetUpcomingEvents(userId, today, take);

    public List<EventItem> GetEventsForMonth(int userId, int year, int month) =>
        _repository.GetEventsForMonth(userId, year, month);

    public EventItem? GetEvent(Guid id, int userId) => _repository.GetEvent(id, userId);

    public void AddEvent(EventItem item) => _repository.AddEvent(item);

    public void UpdateEvent(EventItem item) => _repository.UpdateEvent(item);

    public void DeleteEvent(Guid id, int userId) => _repository.DeleteEvent(id, userId);

    public OperationResult<EventItem> CreateEvent(int userId, EventItem item)
    {
        var title = item.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            return OperationResult<EventItem>.Fail("Event title is required.");
        }

        var normalized = new EventItem
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
        var existing = _repository.GetEvent(item.Id, userId);
        if (existing == null)
        {
            return OperationResult<EventItem>.Fail("Event not found.");
        }

        var title = item.Title?.Trim() ?? string.Empty;
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
        var existing = _repository.GetEvent(id, userId);
        if (existing == null)
        {
            return OperationResult<bool>.Fail("Event not found.");
        }

        _repository.DeleteEvent(id, userId);
        return OperationResult<bool>.Ok(true);
    }

    public CalendarData BuildCurrentMonth()
    {
        var today = DateTime.Today;
        return BuildMonth(today.Year, today.Month);
    }

    public CalendarView BuildCalendarView(int userId, int selectedDay)
    {
        var today = DateTime.Today;
        return BuildCalendarView(userId, today.Year, today.Month, selectedDay);
    }

    public CalendarView BuildCalendarView(int userId, int year, int month, int selectedDay)
    {
        var calendar = BuildMonth(year, month);
        var monthEvents = _repository.GetEventsForMonth(userId, calendar.Year, calendar.Month);

        var safeSelectedDay = Math.Clamp(selectedDay, 1, calendar.Days.Count);

        var selectedEvents = monthEvents
            .Where(e => e.Day == safeSelectedDay)
            .OrderBy(e => e.Time)
            .ToList();

        return new CalendarView
        {
            Calendar = calendar,
            MonthEvents = monthEvents,
            SelectedDayEvents = selectedEvents
        };
    }

    private static CalendarData BuildMonth(int year, int month)
    {
        var safeMonth = Math.Clamp(month, 1, 12);
        var daysInMonth = DateTime.DaysInMonth(year, safeMonth);
        var date = new DateTime(year, safeMonth, 1);

        return new CalendarData
        {
            Year = year,
            Month = safeMonth,
            MonthName = date.ToString("MMMM"),
            Days = Enumerable.Range(1, daysInMonth).ToList()
        };
    }
}
