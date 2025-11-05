using AdministrationPlat.Data;
using AdministrationPlat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class TeacherIndex : PageModel
{
    private readonly ApplicationDbContext _context;

    public TeacherIndex(ApplicationDbContext context)
    {
        _context = context;
    }

    public int ClassCount { get; private set; }
    public int StudentCount { get; private set; }
    public IReadOnlyList<EventItem> UpcomingEvents { get; private set; } = Array.Empty<EventItem>();
    public IReadOnlyList<GradeRecord> RecentGrades { get; private set; } = Array.Empty<GradeRecord>();

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        ClassCount = _context.Classes.Count(c => c.TeacherId == userId);

        StudentCount = _context.Enrollments
            .Where(e => e.SchoolClass != null && e.SchoolClass.TeacherId == userId)
            .Select(e => e.StudentId)
            .Distinct()
            .Count();

        UpcomingEvents = _context.TeacherEvents
            .Where(e => e.UserId == userId)
            .AsEnumerable()
            .Select(e =>
            {
                try
                {
                    _ = new DateTime(e.Year, e.Month, e.Day);
                    return e;
                }
                catch
                {
                    return null;
                }
            })
            .Where(e => e != null)
            .OrderBy(e => new DateTime(e!.Year, e.Month, e.Day))
            .ThenBy(e => e!.Time)
            .Take(5)
            .Select(e => e!)
            .ToList();

        RecentGrades = _context.GradeRecords
            .Include(r => r.Student)
            .Include(r => r.SchoolClass)
            .Where(r => r.SchoolClass != null && r.SchoolClass.TeacherId == userId)
            .OrderByDescending(r => r.DateRecorded)
            .ThenByDescending(r => r.Id)
            .Take(10)
            .ToList();

        return Page();
    }
}
