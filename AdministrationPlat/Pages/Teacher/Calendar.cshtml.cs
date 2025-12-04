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

    //testForGit
    public List<int> Days { get; private set; } = new();
    public int Year { get; private set; }
    public string MonthName { get; private set; } = string.Empty;
    public int Month { get; private set; }

    [BindProperty] public bool ShowOverlay { get; set; }
    [BindProperty] public int SelectedDay { get; set; }
    [BindProperty] public EventItem NewEvent { get; set; } = new();
    [BindProperty] public Guid EditingId { get; set; }

    public List<EventItem> MonthEvents { get; private set; } = new();
    public List<EventItem> SelectedDayEvents { get; private set; } = new();

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedDay = DateTime.Today.Day;
        LoadCalendar(userId.Value);
        return Page();
    }

    public IActionResult OnPostShowOverlay(int selectedDay)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedDay = selectedDay;
        ShowOverlay = true;
        LoadCalendar(userId.Value);
        return Page();
    }

    public IActionResult OnPostHideOverlay()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        ShowOverlay = false;
        LoadCalendar(userId.Value);
        return Page();
    }

    public IActionResult OnPostAddEvent(int selectedDay)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedDay = selectedDay;
        LoadCalendar(userId.Value);

        var toCreate = new EventItem
        {
            Id = Guid.NewGuid(),
            Title = NewEvent.Title,
            Description = NewEvent.Description,
            Location = NewEvent.Location,
            Time = NewEvent.Time,
            Day = selectedDay,
            Month = Month,
            Year = Year
        };

        var result = _logic.CreateEvent(userId.Value, toCreate);
        if (!result.Success)
        {
            ModelState.AddModelError("NewEvent.Title", result.Error ?? "Event title is required.");
            ShowOverlay = true;
            LoadCalendar(userId.Value);
            return Page();
        }

        ResetFormState();
        ShowOverlay = true;
        LoadCalendar(userId.Value);
        return Page();
    }

    public IActionResult OnPostDeleteEvent(Guid id, int selectedDay)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedDay = selectedDay;
        LoadCalendar(userId.Value);

        _logic.DeleteEventForUser(userId.Value, id);

        ShowOverlay = true;
        LoadCalendar(userId.Value);
        return Page();
    }

    public IActionResult OnPostEditEvent(Guid id, int selectedDay)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedDay = selectedDay;
        LoadCalendar(userId.Value);

        var ev = _logic.GetEvent(id, userId.Value);
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
        LoadCalendar(userId.Value);
        return Page();
    }

    public IActionResult OnPostUpdateEvent(int selectedDay)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedDay = selectedDay;
        LoadCalendar(userId.Value);

        var updated = new EventItem
        {
            Id = NewEvent.Id,
            Title = NewEvent.Title,
            Description = NewEvent.Description,
            Location = NewEvent.Location,
            Time = NewEvent.Time,
            Day = selectedDay,
            Month = Month,
            Year = Year
        };

        var result = _logic.UpdateEventDetails(userId.Value, updated);
        if (!result.Success)
        {
            ModelState.AddModelError("NewEvent.Title", result.Error ?? "Unable to update event.");
            ShowOverlay = true;
            LoadCalendar(userId.Value);
            return Page();
        }

        EditingId = Guid.Empty;
        ResetFormState();
        ShowOverlay = true;
        LoadCalendar(userId.Value);
        return Page();
    }

    private void LoadCalendar(int userId)
    {
        var view = _logic.BuildCalendarView(userId, SelectedDay);

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
}
