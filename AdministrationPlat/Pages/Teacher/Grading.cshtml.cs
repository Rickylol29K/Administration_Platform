using AdministrationPlat.Models;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdministrationPlat.Pages.Teacher;

public class Grading : PageModel
{
    private readonly ILogicService _logic;

    public Grading(ILogicService logic)
    {
        _logic = logic;
    }

    public List<SchoolClass> AvailableClasses { get; private set; } = new();
    public string ActiveClassName { get; private set; } = string.Empty;
    public bool SheetLoaded { get; private set; }

    [BindProperty]
    public int SelectedClassId { get; set; }

    [BindProperty]
    public string AssessmentName { get; set; } = string.Empty;

    [BindProperty]
    public DateTime AssessmentDate { get; set; } = DateTime.Today;

    [BindProperty]
    public List<StudentGradeEntry> StudentGrades { get; set; } = new();

    public IActionResult OnGet()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadClasses(userId);
        AssessmentDate = DateTime.Today;
        return Page();
    }

    public IActionResult OnPostLoad()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadClasses(userId);
        AssessmentDate = AssessmentDate.Date;

        if (SelectedClassId == 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Pick a class.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(AssessmentName))
        {
            ModelState.AddModelError(nameof(AssessmentName), "Give the assessment a name.");
            return Page();
        }

        AssessmentName = AssessmentName.Trim();

        FillGradeSheet();
        SheetLoaded = true;
        return Page();
    }

    public IActionResult OnPostSave()
    {
        IActionResult? redirect = EnsureTeacher(out int userId);
        if (redirect != null)
        {
            return redirect;
        }

        LoadClasses(userId);
        AssessmentDate = AssessmentDate.Date;
        if (StudentGrades == null)
        {
            StudentGrades = new List<StudentGradeEntry>();
        }

        if (SelectedClassId == 0)
        {
            ModelState.AddModelError(nameof(SelectedClassId), "Pick a class.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(AssessmentName))
        {
            ModelState.AddModelError(nameof(AssessmentName), "Give the assessment a name.");
            return Page();
        }

        AssessmentName = AssessmentName.Trim();

        bool hasEntryErrors = false;
        for (int i = 0; i < StudentGrades.Count; i++)
        {
            if (StudentGrades[i].Score == null)
            {
                ModelState.AddModelError($"StudentGrades[{i}].Score", "Score is required.");
                hasEntryErrors = true;
            }

            if (string.IsNullOrWhiteSpace(StudentGrades[i].Comment))
            {
                ModelState.AddModelError($"StudentGrades[{i}].Comment", "Comment is required.");
                hasEntryErrors = true;
            }
        }

        if (hasEntryErrors)
        {
            var selectedClass = AvailableClasses.Find(c => c.Id == SelectedClassId);
            if (selectedClass == null || selectedClass.Name == null)
            {
                ActiveClassName = string.Empty;
            }
            else
            {
                ActiveClassName = selectedClass.Name;
            }
            SheetLoaded = true;
            return Page();
        }

        OperationResult<GradeSheet> result = _logic.SaveGrades(
            SelectedClassId,
            AssessmentName,
            AssessmentDate,
            null,
            StudentGrades);

        if (result.Success && result.Value != null)
        {
            TempData["GradingMessage"] = "Grades saved.";
            ActiveClassName = result.Value.ClassName;
            StudentGrades = result.Value.Entries;
            SheetLoaded = true;
            return Page();
        }

        string message;
        if (result.Error == null)
        {
            message = "Unable to save grades.";
        }
        else
        {
            message = result.Error;
        }
        ModelState.AddModelError(string.Empty, message);
        return Page();
    }

    private void LoadClasses(int userId)
    {
        AvailableClasses = _logic.GetClassesForUserOrFallback(userId);
    }

    private void FillGradeSheet()
    {
        OperationResult<GradeSheet> sheetResult = _logic.BuildGradeSheet(SelectedClassId, AssessmentName, AssessmentDate);

        if (!sheetResult.Success || sheetResult.Value == null)
        {
            StudentGrades = new List<StudentGradeEntry>();
            ActiveClassName = string.Empty;
            return;
        }

        ActiveClassName = sheetResult.Value.ClassName;
        StudentGrades = sheetResult.Value.Entries;
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
