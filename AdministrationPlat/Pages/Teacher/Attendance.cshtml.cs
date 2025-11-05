using AdministrationPlat.Data;
using AdministrationPlat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class Attendance : PageModel
{
    private readonly ApplicationDbContext _context;

    public Attendance(ApplicationDbContext context)
    {
        _context = context;
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

        var existing = _context.AttendanceRecords
            .Where(r => r.SchoolClassId == SelectedClassId && r.Date == SelectedDate)
            .ToList();

        var inputs = StudentAttendances.ToDictionary(s => s.StudentId, s => s.IsPresent);

        foreach (var record in existing)
        {
            if (inputs.TryGetValue(record.StudentId, out var isPresent))
            {
                record.IsPresent = isPresent;
            }
            else
            {
                _context.AttendanceRecords.Remove(record);
            }
        }

        var existingIds = existing.Select(r => r.StudentId).ToHashSet();
        foreach (var entry in StudentAttendances)
        {
            if (!existingIds.Contains(entry.StudentId))
            {
                _context.AttendanceRecords.Add(new AttendanceRecord
                {
                    StudentId = entry.StudentId,
                    SchoolClassId = SelectedClassId,
                    Date = SelectedDate,
                    IsPresent = entry.IsPresent
                });
            }
        }

        _context.SaveChanges();

        TempData["AttendanceSaved"] = "Attendance saved.";
        FillRoster();
        RosterLoaded = true;
        return Page();
    }

    private void LoadClasses(int userId)
    {
        AvailableClasses = _context.Classes
            .Where(c => c.TeacherId == userId)
            .OrderBy(c => c.Name)
            .ToList();

        if (AvailableClasses.Count == 0)
        {
            AvailableClasses = _context.Classes
                .OrderBy(c => c.Name)
                .ToList();
        }
    }

    private void FillRoster()
    {
        ActiveClassName = _context.Classes
            .Where(c => c.Id == SelectedClassId)
            .Select(c => c.Name)
            .FirstOrDefault() ?? string.Empty;

        var roster = _context.Enrollments
            .Where(e => e.SchoolClassId == SelectedClassId)
            .Include(e => e.Student)
            .Where(e => e.Student != null)
            .Select(e => e.Student!)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToList();

        var existing = _context.AttendanceRecords
            .Where(r => r.SchoolClassId == SelectedClassId && r.Date == SelectedDate)
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
