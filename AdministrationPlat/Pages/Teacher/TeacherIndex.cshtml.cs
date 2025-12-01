using AdministrationPlat.Models;
using Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class TeacherIndex : PageModel
{
    private readonly ILogicService _logic;

    public TeacherIndex(ILogicService logic)
    {
        _logic = logic;
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

        ClassCount = _logic.GetClassCount(userId.Value);
        StudentCount = _logic.GetDistinctStudentCount(userId.Value);
        UpcomingEvents = _logic.GetUpcomingEvents(userId.Value, DateTime.Today, 5);
        RecentGrades = _logic.GetRecentGrades(userId.Value, 10);

        return Page();
    }
}
