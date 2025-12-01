using AdministrationPlat.Models;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class Attendance : PageModel
{
    private readonly ILogicService _logic;

    public Attendance(ILogicService logic)
    {
        _logic = logic;
    }

    public List<SchoolClass> AvailableClasses { get; private set; } = new();
    public string ActiveClassName { get; private set; } = string.Empty;
    public bool RosterLoaded { get; private set; }

    [BindProperty]
    public int SelectedClassId { get; set; }

    [BindProperty]
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    [BindProperty]
    public List<StudentAttendance> StudentAttendances { get; set; } = new();

    public IActionResult OnGet()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Index");
        }

        LoadClasses(userId);
        SelectedDate = DateTime.Today;
        return Page();
    }

    public IActionResult OnPostLoad()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Index");
        }

        LoadClasses(userId);
        SelectedDate = SelectedDate.Date;

        if (SelectedClassId == 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Choose a class.");
            return Page();
        }

        FillRoster();
        RosterLoaded = true;
        return Page();
    }

    public IActionResult OnPostSave()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Index");
        }

        LoadClasses(userId);
        SelectedDate = SelectedDate.Date;
        StudentAttendances ??= new List<StudentAttendance>();

        if (SelectedClassId == 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Choose a class before saving.");
            return Page();
        }

        var result = _logic.SaveAttendance(SelectedClassId, SelectedDate, StudentAttendances);

        TempData["AttendanceSaved"] = "Attendance saved.";
        if (result.Success && result.Value != null)
        {
            ActiveClassName = result.Value.ClassName;
            StudentAttendances = result.Value.Students;
            RosterLoaded = true;
            return Page();
        }

        ModelState.AddModelError(string.Empty, result.Error ?? "Unable to save attendance.");
        return Page();
    }

    private void LoadClasses(int userId)
    {
        AvailableClasses = _logic.GetClassesForUserOrFallback(userId);
    }

    private void FillRoster()
    {
        var rosterResult = _logic.BuildAttendanceRoster(SelectedClassId, SelectedDate);
        if (!rosterResult.Success || rosterResult.Value == null)
        {
            ActiveClassName = string.Empty;
            StudentAttendances = new List<StudentAttendance>();
            return;
        }

        ActiveClassName = rosterResult.Value.ClassName;
        StudentAttendances = rosterResult.Value.Students;
    }

    private bool TryGetUserId(out int userId)
    {
        userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        return userId != 0;
    }
}
