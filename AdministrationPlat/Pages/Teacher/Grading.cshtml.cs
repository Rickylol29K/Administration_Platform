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
    public decimal? MaxScore { get; set; }

    [BindProperty]
    public List<StudentGradeEntry> StudentGrades { get; set; } = new();

    public IActionResult OnGet()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Index");
        }

        LoadClasses(userId);
        AssessmentDate = DateTime.Today;
        return Page();
    }

    public IActionResult OnPostLoad()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Index");
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
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Index");
        }

        LoadClasses(userId);
        AssessmentDate = AssessmentDate.Date;
        StudentGrades ??= new List<StudentGradeEntry>();

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

        var result = _logic.SaveGrades(
            SelectedClassId,
            AssessmentName,
            AssessmentDate,
            MaxScore,
            StudentGrades);

        if (result.Success && result.Value != null)
        {
            TempData["GradingMessage"] = "Grades saved.";
            ActiveClassName = result.Value.ClassName;
            StudentGrades = result.Value.Entries;
            SheetLoaded = true;
            return Page();
        }

        ModelState.AddModelError(string.Empty, result.Error ?? "Unable to save grades.");
        return Page();
    }

    private void LoadClasses(int userId)
    {
        AvailableClasses = _logic.GetClassesForUserOrFallback(userId);
    }

    private void FillGradeSheet()
    {
        var sheetResult = _logic.BuildGradeSheet(SelectedClassId, AssessmentName, AssessmentDate);

        if (!sheetResult.Success || sheetResult.Value == null)
        {
            StudentGrades = new List<StudentGradeEntry>();
            ActiveClassName = string.Empty;
            return;
        }

        ActiveClassName = sheetResult.Value.ClassName;
        StudentGrades = sheetResult.Value.Entries;
    }

    private bool TryGetUserId(out int userId)
    {
        userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        return userId != 0;
    }

}
