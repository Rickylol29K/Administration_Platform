using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take) =>
        _events.GetUpcomingEvents(userId, today, take);

    public List<EventItem> GetEventsForMonth(int userId, int year, int month) =>
        _events.GetEventsForMonth(userId, year, month);

    public EventItem? GetEvent(Guid id, int userId) => _events.GetEvent(id, userId);

    public void AddEvent(EventItem item) => _events.AddEvent(item);

    public void UpdateEvent(EventItem item) => _events.UpdateEvent(item);

    public void DeleteEvent(Guid id, int userId) => _events.DeleteEvent(id, userId);

    public OperationResult<EventItem> CreateEvent(int userId, EventItem item) => _events.CreateEvent(userId, item);

    public OperationResult<EventItem> UpdateEventDetails(int userId, EventItem item) => _events.UpdateEventDetails(userId, item);

    public OperationResult<bool> DeleteEventForUser(int userId, Guid id) => _events.DeleteEventForUser(userId, id);

    public CalendarData BuildCurrentMonth() => _events.BuildCurrentMonth();

    public CalendarView BuildCalendarView(int userId, int selectedDay) => _events.BuildCalendarView(userId, selectedDay);

    public CalendarView BuildCalendarView(int userId, int year, int month, int selectedDay) =>
        _events.BuildCalendarView(userId, year, month, selectedDay);
}
