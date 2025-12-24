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
    public IReadOnlyList<Announcement> Announcements { get; private set; } = Array.Empty<Announcement>();

    public IActionResult OnGet()
    {
        var redirect = EnsureTeacher(out var userId);
        if (redirect != null)
        {
            return redirect;
        }

        ClassCount = _logic.GetClassCount(userId);
        StudentCount = _logic.GetDistinctStudentCount(userId);
        UpcomingEvents = _logic.GetUpcomingEvents(userId, DateTime.Today, 5);
        RecentGrades = _logic.GetRecentGrades(userId, 10);
        Announcements = _logic.GetAnnouncements(5);

        return Page();
    }

    public IActionResult OnPostDeleteEvent(Guid id)
    {
        var redirect = EnsureTeacher(out var userId);
        if (redirect != null)
        {
            return redirect;
        }

        _logic.DeleteEventForUser(userId, id);

        ClassCount = _logic.GetClassCount(userId);
        StudentCount = _logic.GetDistinctStudentCount(userId);
        UpcomingEvents = _logic.GetUpcomingEvents(userId, DateTime.Today, 5);
        RecentGrades = _logic.GetRecentGrades(userId, 10);
        Announcements = _logic.GetAnnouncements(5);

        return Page();
    }

    private IActionResult? EnsureTeacher(out int userId)
    {
        var sessionUserId = HttpContext.Session.GetInt32("UserId");
        var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;

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
