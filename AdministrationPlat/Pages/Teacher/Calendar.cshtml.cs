using AdministrationPlat.Data;
using AdministrationPlat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class CalendarModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CalendarModel(ApplicationDbContext context)
    {
        _context = context;
    }

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
        BuildCalendar();
        LoadEvents(userId.Value);
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
        BuildCalendar();
        LoadEvents(userId.Value);
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
        BuildCalendar();
        LoadEvents(userId.Value);
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
        BuildCalendar();
        if (!ModelState.IsValid)
        {
            ShowOverlay = true;
            LoadEvents(userId.Value);
            return Page();
        }

        NewEvent.Id = Guid.NewGuid();
        NewEvent.Day = selectedDay;
        NewEvent.Month = Month;
        NewEvent.Year = Year;
        NewEvent.UserId = userId.Value;
        NewEvent.Title = NewEvent.Title.Trim();
        if (string.IsNullOrWhiteSpace(NewEvent.Title))
        {
            ModelState.AddModelError("NewEvent.Title", "Event title is required.");
            ShowOverlay = true;
            LoadEvents(userId.Value);
            return Page();
        }
        NewEvent.Description = string.IsNullOrWhiteSpace(NewEvent.Description)
            ? null
            : NewEvent.Description.Trim();
        NewEvent.Location = string.IsNullOrWhiteSpace(NewEvent.Location)
            ? null
            : NewEvent.Location.Trim();
        NewEvent.Time = string.IsNullOrWhiteSpace(NewEvent.Time)
            ? null
            : NewEvent.Time.Trim();

        _context.TeacherEvents.Add(NewEvent);
        _context.SaveChanges();

        ResetFormState();
        ShowOverlay = true;
        LoadEvents(userId.Value);
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
        BuildCalendar();

        var ev = _context.TeacherEvents.FirstOrDefault(e => e.Id == id && e.UserId == userId.Value);
        if (ev != null)
        {
            _context.TeacherEvents.Remove(ev);
            _context.SaveChanges();
        }

        ShowOverlay = true;
        LoadEvents(userId.Value);
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
        BuildCalendar();

        var ev = _context.TeacherEvents.FirstOrDefault(e => e.Id == id && e.UserId == userId.Value);
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
        LoadEvents(userId.Value);
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
        BuildCalendar();
        if (!ModelState.IsValid)
        {
            ShowOverlay = true;
            LoadEvents(userId.Value);
            return Page();
        }

        var ev = _context.TeacherEvents.FirstOrDefault(e => e.Id == NewEvent.Id && e.UserId == userId.Value);
        if (ev != null)
        {
            var title = NewEvent.Title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("NewEvent.Title", "Event title is required.");
                ShowOverlay = true;
                LoadEvents(userId.Value);
                return Page();
            }

            ev.Title = title;
            ev.Description = string.IsNullOrWhiteSpace(NewEvent.Description)
                ? null
                : NewEvent.Description.Trim();
            ev.Location = string.IsNullOrWhiteSpace(NewEvent.Location)
                ? null
                : NewEvent.Location.Trim();
            ev.Time = string.IsNullOrWhiteSpace(NewEvent.Time)
                ? null
                : NewEvent.Time.Trim();
            _context.SaveChanges();
        }

        EditingId = Guid.Empty;
        ResetFormState();
        ShowOverlay = true;
        LoadEvents(userId.Value);
        return Page();
    }

    private void BuildCalendar()
    {
        var today = DateTime.Today;
        Year = today.Year;
        Month = today.Month;
        MonthName = today.ToString("MMMM");

        var daysInMonth = DateTime.DaysInMonth(Year, Month);
        Days = Enumerable.Range(1, daysInMonth).ToList();
    }

    private void LoadEvents(int userId)
    {
        MonthEvents = _context.TeacherEvents
            .Where(e => e.Month == Month && e.Year == Year && e.UserId == userId)
            .OrderBy(e => e.Day)
            .ThenBy(e => e.Time)
            .ToList();

        SelectedDayEvents = MonthEvents
            .Where(e => e.Day == SelectedDay)
            .OrderBy(e => e.Time)
            .ToList();
    }

    private void ResetFormState()
    {
        NewEvent = new EventItem();
        EditingId = Guid.Empty;
        ModelState.Clear();
    }
}
