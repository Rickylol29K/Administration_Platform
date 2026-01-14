using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public CalendarData BuildCurrentMonth()
    {
        DateTime today = DateTime.Today;
        return BuildMonth(today.Year, today.Month);
    }

    public CalendarView BuildCalendarView(int userId, int selectedDay)
    {
        DateTime today = DateTime.Today;
        return BuildCalendarView(userId, today.Year, today.Month, selectedDay);
    }

    public CalendarView BuildCalendarView(int userId, int year, int month, int selectedDay)
    {
        CalendarData calendar = BuildMonth(year, month);
        List<EventItem> monthEvents = _repository.GetEventsForMonth(userId, calendar.Year, calendar.Month);

        int safeSelectedDay = Math.Clamp(selectedDay, 1, calendar.Days.Count);
        List<EventItem> selectedEvents = new List<EventItem>();
        foreach (EventItem item in monthEvents)
        {
            if (item.Day == safeSelectedDay)
            {
                selectedEvents.Add(item);
            }
        }

        selectedEvents.Sort((left, right) => string.Compare(left.Time, right.Time, StringComparison.Ordinal));

        return new CalendarView
        {
            Calendar = calendar,
            MonthEvents = monthEvents,
            SelectedDayEvents = selectedEvents
        };
    }

    private static CalendarData BuildMonth(int year, int month)
    {
        int safeMonth = Math.Clamp(month, 1, 12);
        int daysInMonth = DateTime.DaysInMonth(year, safeMonth);
        DateTime date = new DateTime(year, safeMonth, 1);

        List<int> days = new List<int>();
        for (int day = 1; day <= daysInMonth; day++)
        {
            days.Add(day);
        }

        return new CalendarData
        {
            Year = year,
            Month = safeMonth,
            MonthName = date.ToString("MMMM"),
            Days = days
        };
    }
}
