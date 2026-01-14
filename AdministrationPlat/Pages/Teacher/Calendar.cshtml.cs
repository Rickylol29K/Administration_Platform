using AdministrationPlat.Models;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class CalendarModel : PageModel
{
    private readonly ILogicService _logic;

    public CalendarModel(ILogicService logic)
    {
        _logic = logic;
    }

    public List<int> Days { get; private set; } = new();
    public int Year { get; private set; }
    public string MonthName { get; private set; } = string.Empty;
    public int Month { get; private set; }

    [BindProperty]
    public bool ShowOverlay { get; set; }

    [BindProperty]
    public int SelectedDay { get; set; }

    [BindProperty]
    public int SelectedMonth { get; set; }

    [BindProperty]
    public int SelectedYear { get; set; }

    [BindProperty]
    public EventItem NewEvent { get; set; } = new();

    [BindProperty]
    public Guid EditingId { get; set; }

    public List<EventItem> MonthEvents { get; private set; } = new();
    public List<EventItem> SelectedDayEvents { get; private set; } = new();

    public IActionResult OnGet()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SelectedDay = DateTime.Today.Day;
        SelectedMonth = DateTime.Today.Month;
        SelectedYear = DateTime.Today.Year;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostShowOverlay(int selectedDay, int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        SelectedDay = selectedDay;
        ShowOverlay = true;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostHideOverlay(int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        ShowOverlay = false;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostPreviousMonth(int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        MoveMonth(-1);
        SelectedDay = 1;
        ShowOverlay = false;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostNextMonth(int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        MoveMonth(1);
        SelectedDay = 1;
        ShowOverlay = false;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostAddEvent(int selectedDay, int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        SelectedDay = selectedDay;
        LoadCalendar(userId);

        EventItem toCreate = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = NewEvent.Title,
            Description = NewEvent.Description,
            Location = NewEvent.Location,
            Time = NewEvent.Time,
            Day = selectedDay,
            Month = SelectedMonth,
            Year = SelectedYear
        };

        OperationResult<EventItem> result = _logic.CreateEvent(userId, toCreate);
        if (!result.Success)
        {
            ModelState.AddModelError("NewEvent.Title", result.Error ?? "Event title is required.");
            ShowOverlay = true;
            LoadCalendar(userId);
            return Page();
        }

        ResetFormState();
        ShowOverlay = true;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostDeleteEvent(Guid id, int selectedDay, int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        SelectedDay = selectedDay;
        LoadCalendar(userId);

        _logic.DeleteEventForUser(userId, id);

        ShowOverlay = true;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostEditEvent(Guid id, int selectedDay, int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        SelectedDay = selectedDay;
        LoadCalendar(userId);

        EventItem? ev = _logic.GetEvent(id, userId);
        if (ev != null)
        {
            EditingId = id;
            NewEvent = new EventItem
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Location = ev.Location,
                Time = ev.Time,
                Day = ev.Day,
                Month = ev.Month,
                Year = ev.Year
            };
            ModelState.Clear();
        }

        ShowOverlay = true;
        LoadCalendar(userId);
        return Page();
    }

    public IActionResult OnPostUpdateEvent(int selectedDay, int selectedMonth, int selectedYear)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SyncMonthSelection(selectedMonth, selectedYear);
        SelectedDay = selectedDay;
        LoadCalendar(userId);

        EventItem updated = new EventItem
        {
            Id = NewEvent.Id,
            Title = NewEvent.Title,
            Description = NewEvent.Description,
            Location = NewEvent.Location,
            Time = NewEvent.Time,
            Day = selectedDay,
            Month = SelectedMonth,
            Year = SelectedYear
        };

        OperationResult<EventItem> result = _logic.UpdateEventDetails(userId, updated);
        if (!result.Success)
        {
            ModelState.AddModelError("NewEvent.Title", result.Error ?? "Unable to update event.");
            ShowOverlay = true;
            LoadCalendar(userId);
            return Page();
        }

        EditingId = Guid.Empty;
        ResetFormState();
        ShowOverlay = true;
        LoadCalendar(userId);
        return Page();
    }

    private void LoadCalendar(int userId)
    {
        if (SelectedMonth == 0 || SelectedYear == 0)
        {
            SelectedMonth = DateTime.Today.Month;
            SelectedYear = DateTime.Today.Year;
        }

        CalendarView view = _logic.BuildCalendarView(userId, SelectedYear, SelectedMonth, SelectedDay);

        Year = view.Calendar.Year;
        Month = view.Calendar.Month;
        MonthName = view.Calendar.MonthName;
        Days = view.Calendar.Days;
        MonthEvents = view.MonthEvents;
        SelectedDayEvents = view.SelectedDayEvents;
    }

    private void ResetFormState()
    {
        NewEvent = new EventItem();
        EditingId = Guid.Empty;
        ModelState.Clear();
    }

    private void MoveMonth(int delta)
    {
        if (SelectedMonth == 0 || SelectedYear == 0)
        {
            SelectedMonth = DateTime.Today.Month;
            SelectedYear = DateTime.Today.Year;
        }

        DateTime date = new DateTime(SelectedYear, SelectedMonth, 1).AddMonths(delta);
        SelectedMonth = date.Month;
        SelectedYear = date.Year;
    }

    private void SyncMonthSelection(int selectedMonth, int selectedYear)
    {
        if (selectedMonth >= 1 && selectedMonth <= 12)
        {
            SelectedMonth = selectedMonth;
        }

        if (selectedYear > 0)
        {
            SelectedYear = selectedYear;
        }
    }

    private IActionResult? EnsureTeacher(out int userId)
    {
        int? sessionUserId = HttpContext.Session.GetInt32("UserId");
        bool isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;

        if (!sessionUserId.HasValue)
        {
            userId = 0;
            return RedirectToPage("/Index");
        }

        if (isAdmin)
        {
            userId = 0;
            return RedirectToPage("/Admin/AdminIndex");
        }

        userId = sessionUserId.Value;
        return null;
    }
}
