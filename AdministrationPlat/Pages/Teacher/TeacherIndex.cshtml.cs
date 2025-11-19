using AdministrationPlat.Models;
using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Shared.Teacher;

public class TeacherIndex : PageModel
{
    private readonly IDataRepository _repository;

    public TeacherIndex(IDataRepository repository)
    {
        _repository = repository;
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

        ClassCount = _repository.GetClassCount(userId.Value);
        StudentCount = _repository.GetDistinctStudentCount(userId.Value);
        UpcomingEvents = _repository.GetUpcomingEvents(userId.Value, DateTime.Today, 5);
        RecentGrades = _repository.GetRecentGrades(userId.Value, 10);

        return Page();
    }
}
