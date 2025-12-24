using AdministrationPlat.Models;
using Logic.Models;

namespace Logic.Contracts;

public interface IEventLogic
{
    List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take);
    List<EventItem> GetEventsForMonth(int userId, int year, int month);
    EventItem? GetEvent(Guid id, int userId);
    void AddEvent(EventItem item);
    void UpdateEvent(EventItem item);
    void DeleteEvent(Guid id, int userId);
    OperationResult<EventItem> CreateEvent(int userId, EventItem item);
    OperationResult<EventItem> UpdateEventDetails(int userId, EventItem item);
    OperationResult<bool> DeleteEventForUser(int userId, Guid id);
    CalendarData BuildCurrentMonth();
    CalendarView BuildCalendarView(int userId, int selectedDay);
    CalendarView BuildCalendarView(int userId, int year, int month, int selectedDay);
}
