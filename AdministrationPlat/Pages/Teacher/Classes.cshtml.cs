using AdministrationPlat.Models;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class Classes : PageModel
{
    private readonly ILogicService _logic;

    public Classes(ILogicService logic)
    {
        _logic = logic;
    }

    public List<SchoolClass> TeacherClasses { get; private set; } = new();
    public SchoolClass? ActiveClass { get; private set; }
    public List<ClassEnrollment> ActiveEnrollments { get; private set; } = new();

    [BindProperty]
    public int SelectedClassId { get; set; }

    [BindProperty]
    public bool ShowOverlay { get; set; }

    [BindProperty]
    public string NewClassName { get; set; } = string.Empty;

    [BindProperty]
    public string? NewClassRoom { get; set; }

    [BindProperty]
    public string? NewClassDescription { get; set; }

    [BindProperty]
    public string NewStudentFirstName { get; set; } = string.Empty;

    [BindProperty]
    public string NewStudentLastName { get; set; } = string.Empty;

    [BindProperty]
    public string? NewStudentEmail { get; set; }

    public IActionResult OnGet()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadTeacherClasses(userId);
        return Page();
    }

    public IActionResult OnPostAddClass()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadTeacherClasses(userId);
        ModelState.AddModelError(string.Empty, "Only admins can create classes.");
        return Page();
    }

    public IActionResult OnPostShowOverlay(int classId)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        SelectedClassId = classId;
        LoadTeacherClasses(userId);
        OperationResult<ClassOverlay> overlayResult = _logic.LoadClassOverlay(SelectedClassId, userId);
        if (!overlayResult.Success || overlayResult.Value?.ActiveClass == null)
        {
            ModelState.AddModelError(string.Empty, overlayResult.Error ?? "Unable to load the requested class.");
            ShowOverlay = false;
            return Page();
        }

        ActiveClass = overlayResult.Value.ActiveClass;
        ActiveEnrollments = overlayResult.Value.Enrollments;

        ShowOverlay = true;
        return Page();
    }

    public IActionResult OnPostAddStudent()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadTeacherClasses(userId);
        ModelState.AddModelError(string.Empty, "Only admins can add students.");
        LoadOverlay(userId, SelectedClassId);
        ShowOverlay = true;
        return Page();
    }

    public IActionResult OnPostRemoveStudent(int enrollmentId)
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadTeacherClasses(userId);
        ModelState.AddModelError(string.Empty, "Only admins can remove students.");
        ShowOverlay = true;
        return Page();
    }

    public IActionResult OnPostHideOverlay()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadTeacherClasses(userId);
        ShowOverlay = false;
        return Page();
    }

    private void LoadTeacherClasses(int userId)
    {
        TeacherClasses = _logic.GetClassesForTeacher(userId);
    }

    private void LoadOverlay(int userId, int classId)
    {
        OperationResult<ClassOverlay> overlay = _logic.LoadClassOverlay(classId, userId);
        if (overlay.Success && overlay.Value != null)
        {
            ActiveClass = overlay.Value.ActiveClass;
            ActiveEnrollments = overlay.Value.Enrollments;
        }
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
