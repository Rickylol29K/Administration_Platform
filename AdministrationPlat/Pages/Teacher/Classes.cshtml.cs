using AdministrationPlat.Models;
using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class Classes : PageModel
{
    private readonly IDataRepository _repository;

    public Classes(IDataRepository repository)
    {
        _repository = repository;
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

        var schoolClass = new SchoolClass
        {
            Name = NewClassName.Trim(),
            Room = string.IsNullOrWhiteSpace(NewClassRoom) ? null : NewClassRoom.Trim(),
            Description = string.IsNullOrWhiteSpace(NewClassDescription) ? null : NewClassDescription.Trim(),
            TeacherId = userId.Value
        };

        _repository.AddClass(schoolClass);

        TempData["ClassMessage"] = $"Class \"{schoolClass.Name}\" created.";

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
        LoadActiveClass(userId.Value);

        if (ActiveClass == null)
        {
            ModelState.AddModelError(string.Empty, "Unable to load the requested class.");
            ShowOverlay = false;
            return Page();
        }

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
        LoadActiveClass(userId.Value);

        if (ActiveClass == null)
        {
            ModelState.AddModelError(string.Empty, "Class not found.");
            return Page();
        }

        ShowOverlay = true;

        if (string.IsNullOrWhiteSpace(NewStudentFirstName) || string.IsNullOrWhiteSpace(NewStudentLastName))
        {
            ModelState.AddModelError(string.Empty, "Student first and last name are required.");
            return Page();
        }

        Student? student = null;
        if (!string.IsNullOrWhiteSpace(NewStudentEmail))
        {
            student = _repository.GetStudentByEmail(NewStudentEmail.Trim());
        }

        if (student == null)
        {
            student = new Student
            {
                FirstName = NewStudentFirstName.Trim(),
                LastName = NewStudentLastName.Trim(),
                Email = string.IsNullOrWhiteSpace(NewStudentEmail) ? null : NewStudentEmail.Trim()
            };
            _repository.AddStudent(student);
        }

        var alreadyEnrolled = _repository.EnrollmentExists(student.Id, SelectedClassId);

        if (!alreadyEnrolled)
        {
            _repository.AddEnrollment(student.Id, SelectedClassId);
            TempData["ClassMessage"] = $"{student.FullName} added to {ActiveClass.Name}.";
        }
        else
        {
            TempData["ClassMessage"] = $"{student.FullName} is already enrolled in this class.";
        }

        ModelState.Clear();
        NewStudentFirstName = string.Empty;
        NewStudentLastName = string.Empty;
        NewStudentEmail = null;

        LoadActiveClass(userId.Value);
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

        var enrollment = _repository.GetEnrollmentWithDetails(enrollmentId, userId.Value);

        if (enrollment != null)
        {
            SelectedClassId = enrollment.SchoolClassId;
            _repository.RemoveEnrollment(enrollmentId);
            TempData["ClassMessage"] = $"{enrollment.Student?.FullName ?? "Student"} removed from {enrollment.SchoolClass?.Name}.";
        }

        LoadActiveClass(userId.Value);
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
        TeacherClasses = _repository.GetClassesForTeacher(userId);
    }

    private void LoadActiveClass(int userId)
    {
        ActiveClass = _repository.GetClassWithEnrollments(SelectedClassId, userId);

        ActiveEnrollments = ActiveClass?.Enrollments
            .Where(e => e.Student != null)
            .OrderBy(e => e.Student!.LastName)
            .ThenBy(e => e.Student!.FirstName)
            .ToList() ?? new List<ClassEnrollment>();
    }
}
