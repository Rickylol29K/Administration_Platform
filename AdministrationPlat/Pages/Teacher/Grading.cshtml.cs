using AdministrationPlat.Data;
using AdministrationPlat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdministrationPlat.Pages.Teacher;

public class Grading : PageModel
{
    private readonly ApplicationDbContext _context;

    public Grading(ApplicationDbContext context)
    {
        _context = context;
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
    public List<StudentGradeInput> StudentGrades { get; set; } = new();

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
        StudentGrades ??= new List<StudentGradeInput>();

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

        var existing = _context.GradeRecords
            .Where(r => r.SchoolClassId == SelectedClassId &&
                        r.Assessment == AssessmentName &&
                        r.DateRecorded == AssessmentDate)
            .ToList();

        var inputLookup = StudentGrades.ToDictionary(g => g.StudentId, g => g);

        foreach (var record in existing)
        {
            if (inputLookup.TryGetValue(record.StudentId, out var input) && input.Score.HasValue)
            {
                record.Score = input.Score;
                record.MaxScore = MaxScore;
                record.Comments = string.IsNullOrWhiteSpace(input.Comment) ? null : input.Comment.Trim();
            }
            else
            {
                _context.GradeRecords.Remove(record);
            }
        }

        var storedIds = existing.Select(r => r.StudentId).ToHashSet();
        foreach (var grade in StudentGrades)
        {
            if (!grade.Score.HasValue || storedIds.Contains(grade.StudentId))
            {
                continue;
            }

            _context.GradeRecords.Add(new GradeRecord
            {
                StudentId = grade.StudentId,
                SchoolClassId = SelectedClassId,
                Assessment = AssessmentName,
                DateRecorded = AssessmentDate,
                Score = grade.Score,
                MaxScore = MaxScore,
                Comments = string.IsNullOrWhiteSpace(grade.Comment) ? null : grade.Comment.Trim()
            });
        }

        _context.SaveChanges();

        TempData["GradingMessage"] = "Grades saved.";
        FillGradeSheet();
        SheetLoaded = true;
        return Page();
    }

    private void LoadClasses(int userId)
    {
        AvailableClasses = _context.Classes
            .Where(c => c.TeacherId == userId)
            .OrderBy(c => c.Name)
            .ToList();

        if (AvailableClasses.Count == 0)
        {
            AvailableClasses = _context.Classes
                .OrderBy(c => c.Name)
                .ToList();
        }
    }

    private void FillGradeSheet()
    {
        var classInfo = _context.Classes
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefault(c => c.Id == SelectedClassId);

        if (classInfo == null)
        {
            StudentGrades = new List<StudentGradeInput>();
            ActiveClassName = string.Empty;
            return;
        }

        ActiveClassName = classInfo.Name;

        var students = classInfo.Enrollments
            .Where(e => e.Student != null)
            .Select(e => e.Student!)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToList();

        var existing = _context.GradeRecords
            .Where(r => r.SchoolClassId == SelectedClassId &&
                        r.Assessment == AssessmentName &&
                        r.DateRecorded == AssessmentDate)
            .ToDictionary(r => r.StudentId, r => r);

        StudentGrades = students
            .Select(student => new StudentGradeInput
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                Score = existing.TryGetValue(student.Id, out var record) ? record.Score : null,
                Comment = existing.TryGetValue(student.Id, out var record2) ? record2.Comments : null
            })
            .ToList();
    }

    private bool TryGetUserId(out int userId)
    {
        userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        return userId != 0;
    }

    public class StudentGradeInput
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string? Comment { get; set; }
    }
}
