using AdministrationPlat.Models;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Admin;

public class AdminIndex : PageModel
{
    private readonly ILogicService _logic;

    public AdminIndex(ILogicService logic)
    {
        _logic = logic;
    }

    public List<User> Teachers { get; private set; } = new();
    public List<SchoolClass> Classes { get; private set; } = new();
    public List<Announcement> Announcements { get; private set; } = new();

    public IReadOnlyDictionary<int, string> TeacherLookup { get; private set; } = new Dictionary<int, string>();

    [BindProperty]
    public string NewTeacherUsername { get; set; } = string.Empty;

    [BindProperty]
    public string NewTeacherPassword { get; set; } = string.Empty;

    [BindProperty]
    public int SelectedTeacherId { get; set; }

    [BindProperty]
    public string NewClassName { get; set; } = string.Empty;

    [BindProperty]
    public string? NewClassRoom { get; set; }

    [BindProperty]
    public string? NewClassDescription { get; set; }

    [BindProperty]
    public int SelectedClassId { get; set; }

    [BindProperty]
    public string NewStudentFirstName { get; set; } = string.Empty;

    [BindProperty]
    public string NewStudentLastName { get; set; } = string.Empty;

    [BindProperty]
    public string? NewStudentEmail { get; set; }

    [BindProperty]
    public string AnnouncementTitle { get; set; } = string.Empty;

    [BindProperty]
    public string? AnnouncementBody { get; set; }

    public IActionResult OnGet()
    {
        if (!TryGetAdminUserId(out _))
        {
            return RedirectToPage("/Index");
        }

        LoadAdminData();
        return Page();
    }

    public IActionResult OnPostCreateTeacher()
    {
        if (!TryGetAdminUserId(out _))
        {
            return RedirectToPage("/Index");
        }

        OperationResult<User> result = _logic.Register(NewTeacherUsername, NewTeacherPassword, false);
        if (!result.Success)
        {
            string message;
            if (result.Error == null)
            {
                message = "Unable to create teacher account.";
            }
            else
            {
                message = result.Error;
            }
            ModelState.AddModelError(string.Empty, message);
            LoadAdminData();
            return Page();
        }

        TempData["AdminMessage"] = $"Teacher account \"{result.Value?.Username}\" created.";

        ModelState.Clear();
        NewTeacherUsername = string.Empty;
        NewTeacherPassword = string.Empty;

        LoadAdminData();
        return Page();
    }

    public IActionResult OnPostCreateClass()
    {
        if (!TryGetAdminUserId(out _))
        {
            return RedirectToPage("/Index");
        }

        if (SelectedTeacherId <= 0)
        {
            ModelState.AddModelError(nameof(SelectedTeacherId), "Select a teacher for this class.");
            LoadAdminData();
            return Page();
        }

        User? teacher = _logic.GetUserById(SelectedTeacherId);
        if (teacher == null || teacher.IsAdmin)
        {
            ModelState.AddModelError(nameof(SelectedTeacherId), "Selected teacher is not valid.");
            LoadAdminData();
            return Page();
        }

        OperationResult<SchoolClass> result = _logic.CreateClass(SelectedTeacherId, NewClassName, NewClassRoom, NewClassDescription);
        if (!result.Success || result.Value == null)
        {
            string message;
            if (result.Error == null)
            {
                message = "Unable to create class.";
            }
            else
            {
                message = result.Error;
            }
            ModelState.AddModelError(nameof(NewClassName), message);
            LoadAdminData();
            return Page();
        }

        TempData["AdminMessage"] = $"Class \"{result.Value.Name}\" created for {teacher.Username}.";

        ModelState.Clear();
        NewClassName = string.Empty;
        NewClassRoom = null;
        NewClassDescription = null;

        LoadAdminData();
        return Page();
    }

    public IActionResult OnPostAddStudent()
    {
        if (!TryGetAdminUserId(out _))
        {
            return RedirectToPage("/Index");
        }

        if (SelectedClassId <= 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Select a class for the student.");
            LoadAdminData();
            return Page();
        }

        ClassMembershipResult result = _logic.AddStudentToClassAsAdmin(
            SelectedClassId,
            NewStudentFirstName,
            NewStudentLastName,
            NewStudentEmail);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            LoadAdminData();
            return Page();
        }

        TempData["AdminMessage"] = result.Message;

        ModelState.Clear();
        NewStudentFirstName = string.Empty;
        NewStudentLastName = string.Empty;
        NewStudentEmail = null;

        LoadAdminData();
        return Page();
    }

    public IActionResult OnPostAddAnnouncement()
    {
        if (!TryGetAdminUserId(out int adminId))
        {
            return RedirectToPage("/Index");
        }

        OperationResult<Announcement> result = _logic.CreateAnnouncement(adminId, AnnouncementTitle, AnnouncementBody);
        if (!result.Success)
        {
            string message;
            if (result.Error == null)
            {
                message = "Unable to create announcement.";
            }
            else
            {
                message = result.Error;
            }
            ModelState.AddModelError(nameof(AnnouncementTitle), message);
            LoadAdminData();
            return Page();
        }

        TempData["AdminMessage"] = "Announcement published.";

        ModelState.Clear();
        AnnouncementTitle = string.Empty;
        AnnouncementBody = null;

        LoadAdminData();
        return Page();
    }

    public IActionResult OnPostDeleteAnnouncement(Guid announcementId)
    {
        if (!TryGetAdminUserId(out _))
        {
            return RedirectToPage("/Index");
        }

        OperationResult<bool> result = _logic.DeleteAnnouncement(announcementId);
        if (!result.Success)
        {
            string message;
            if (result.Error == null)
            {
                message = "Unable to remove announcement.";
            }
            else
            {
                message = result.Error;
            }
            ModelState.AddModelError(string.Empty, message);
            LoadAdminData();
            return Page();
        }

        TempData["AdminMessage"] = "Announcement removed.";

        LoadAdminData();
        return Page();
    }

    private bool TryGetAdminUserId(out int adminId)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        bool isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;

        if (!userId.HasValue || !isAdmin)
        {
            adminId = 0;
            return false;
        }

        adminId = userId.Value;
        return true;
    }

    private void LoadAdminData()
    {
        Teachers = _logic.GetTeachers();
        Classes = _logic.GetAllClasses();
        Announcements = _logic.GetAllAnnouncements();
        Dictionary<int, string> lookup = new Dictionary<int, string>();
        foreach (User teacher in Teachers)
        {
            lookup[teacher.Id] = teacher.Username;
        }

        TeacherLookup = lookup;
    }
}
