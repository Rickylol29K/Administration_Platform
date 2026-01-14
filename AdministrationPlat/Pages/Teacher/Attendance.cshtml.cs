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
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadClasses(userId);
        SelectedDate = DateTime.Today;
        return Page();
    }

    public IActionResult OnPostLoad()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
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
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadClasses(userId);
        SelectedDate = SelectedDate.Date;
        if (StudentAttendances == null)
        {
            StudentAttendances = new List<StudentAttendance>();
        }

        if (SelectedClassId == 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Choose a class before saving.");
            return Page();
        }

        OperationResult<AttendanceRoster> result = _logic.SaveAttendance(SelectedClassId, SelectedDate, StudentAttendances);

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
        OperationResult<AttendanceRoster> rosterResult = _logic.BuildAttendanceRoster(SelectedClassId, SelectedDate);
        if (!rosterResult.Success || rosterResult.Value == null)
        {
            ActiveClassName = string.Empty;
            StudentAttendances = new List<StudentAttendance>();
            return;
        }

        ActiveClassName = rosterResult.Value.ClassName;
        StudentAttendances = rosterResult.Value.Students;
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
