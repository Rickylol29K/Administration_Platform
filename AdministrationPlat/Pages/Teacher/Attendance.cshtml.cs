using AdministrationPlat.Models;
using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class Attendance : PageModel
{
    private readonly IDataRepository _repository;

    public Attendance(IDataRepository repository)
    {
        _repository = repository;
    }

    public List<SchoolClass> AvailableClasses { get; private set; } = new();
    public string ActiveClassName { get; private set; } = string.Empty;
    public bool RosterLoaded { get; private set; }

    [BindProperty]
    public int SelectedClassId { get; set; }

    [BindProperty]
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    [BindProperty]
    public List<StudentAttendanceInput> StudentAttendances { get; set; } = new();

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
        StudentAttendances ??= new List<StudentAttendanceInput>();

        if (SelectedClassId == 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Choose a class before saving.");
            return Page();
        }

        _repository.SaveAttendanceRecords(
            SelectedClassId,
            SelectedDate,
            StudentAttendances.Select(a => (a.StudentId, a.IsPresent)));

        TempData["AttendanceSaved"] = "Attendance saved.";
        FillRoster();
        RosterLoaded = true;
        return Page();
    }

    private void LoadClasses(int userId)
    {
        AvailableClasses = _repository.GetClassesForTeacher(userId);

        if (AvailableClasses.Count == 0)
        {
            AvailableClasses = _repository.GetAllClasses();
        }
    }

    private void FillRoster()
    {
        ActiveClassName = _repository.GetClassName(SelectedClassId) ?? string.Empty;

        var roster = _repository.GetStudentsForClass(SelectedClassId);

        var existing = _repository.GetAttendanceRecords(SelectedClassId, SelectedDate)
            .ToDictionary(r => r.StudentId, r => r.IsPresent);

        StudentAttendances = roster
            .Select(student => new StudentAttendanceInput
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                IsPresent = existing.TryGetValue(student.Id, out var status) && status
            })
            .ToList();
    }

    private bool TryGetUserId(out int userId)
    {
        userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        return userId != 0;
    }

    public class StudentAttendanceInput
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }
}
