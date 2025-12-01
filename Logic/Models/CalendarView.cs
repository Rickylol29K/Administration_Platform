using AdministrationPlat.Models;

namespace Logic.Models;

public class CalendarData
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public List<int> Days { get; init; } = new();
}

public class CalendarView
{
    public CalendarData Calendar { get; init; } = new();
    public List<EventItem> MonthEvents { get; init; } = new();
    public List<EventItem> SelectedDayEvents { get; init; } = new();
}
