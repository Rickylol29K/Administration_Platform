using AdministrationPlat.Models;
using Logic;
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
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        LoadTeacherClasses(userId.Value);
        return Page();
    }

    public IActionResult OnPostAddClass()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        LoadTeacherClasses(userId.Value);

        if (string.IsNullOrWhiteSpace(NewClassName))
        {
            ModelState.AddModelError(nameof(NewClassName), "Class name is required.");
            return Page();
        }

        var result = _logic.CreateClass(
            userId.Value,
            NewClassName,
            NewClassRoom,
            NewClassDescription);

        if (!result.Success || result.Value == null)
        {
            ModelState.AddModelError(nameof(NewClassName), result.Error ?? "Unable to create class.");
            return Page();
        }

        TempData["ClassMessage"] = $"Class \"{result.Value.Name}\" created.";

        ModelState.Clear();
        NewClassName = string.Empty;
        NewClassRoom = null;
        NewClassDescription = null;

        LoadTeacherClasses(userId.Value);
        return Page();
    }

    public IActionResult OnPostShowOverlay(int classId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        SelectedClassId = classId;
        LoadTeacherClasses(userId.Value);
        var overlayResult = _logic.LoadClassOverlay(SelectedClassId, userId.Value);
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
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        LoadTeacherClasses(userId.Value);
        var result = _logic.AddStudentToClass(
            userId.Value,
            SelectedClassId,
            NewStudentFirstName,
            NewStudentLastName,
            NewStudentEmail);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ShowOverlay = true;
            LoadTeacherClasses(userId.Value);
            if (result.Overlay.ActiveClass != null)
            {
                ActiveClass = result.Overlay.ActiveClass;
                ActiveEnrollments = result.Overlay.Enrollments;
            }
            return Page();
        }

        TempData["ClassMessage"] = result.Message;

        ModelState.Clear();
        NewStudentFirstName = string.Empty;
        NewStudentLastName = string.Empty;
        NewStudentEmail = null;

        LoadOverlay(userId.Value, SelectedClassId);
        ShowOverlay = true;
        return Page();
    }

    public IActionResult OnPostRemoveStudent(int enrollmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        LoadTeacherClasses(userId.Value);

        var result = _logic.RemoveStudentFromClass(userId.Value, enrollmentId);

        if (result.Success)
        {
            TempData["ClassMessage"] = result.Message;
            SelectedClassId = result.Overlay.ActiveClass?.Id ?? SelectedClassId;
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.Message);
        }

        if (result.Overlay.ActiveClass != null)
        {
            ActiveClass = result.Overlay.ActiveClass;
            ActiveEnrollments = result.Overlay.Enrollments;
        }

        ShowOverlay = true;
        return Page();
    }

    public IActionResult OnPostHideOverlay()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Index");
        }

        LoadTeacherClasses(userId.Value);
        ShowOverlay = false;
        return Page();
    }

    private void LoadTeacherClasses(int userId)
    {
        TeacherClasses = _logic.GetClassesForTeacher(userId);
    }

    private void LoadActiveClass(int userId)
    {
        LoadOverlay(userId, SelectedClassId);
    }

    private void LoadOverlay(int userId, int classId)
    {
        var overlay = _logic.LoadClassOverlay(classId, userId);
        if (overlay.Success && overlay.Value != null)
        {
            ActiveClass = overlay.Value.ActiveClass;
            ActiveEnrollments = overlay.Value.Enrollments;
        }
    }
}
