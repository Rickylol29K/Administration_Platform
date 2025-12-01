using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date) =>
        _repository.GetGradeRecords(classId, assessment, date);

    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records) =>
        _repository.SaveGradeRecords(classId, assessment, date, maxScore, records);

    public List<GradeRecord> GetRecentGrades(int teacherId, int take) => _repository.GetRecentGrades(teacherId, take);

    public OperationResult<GradeSheet> BuildGradeSheet(int classId, string assessment, DateTime date)
    {
        var classInfo = _repository.GetClassWithEnrollments(classId);

        if (classInfo == null)
        {
            return OperationResult<GradeSheet>.Fail("Class not found.");
        }

        var students = classInfo.Enrollments
            .Where(e => e.Student != null)
            .Select(e => e.Student!)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToList();

        var existing = _repository.GetGradeRecords(classId, assessment, date)
            .ToDictionary(r => r.StudentId, r => r);

        var sheet = new GradeSheet
        {
            ClassName = classInfo.Name,
            Entries = students
                .Select(student => new StudentGradeEntry
                {
                    StudentId = student.Id,
                    StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                    Score = existing.TryGetValue(student.Id, out var record) ? record.Score : null,
                    Comment = existing.TryGetValue(student.Id, out var record2) ? record2.Comments : null
                })
                .ToList()
        };

        return OperationResult<GradeSheet>.Ok(sheet);
    }

    public OperationResult<GradeSheet> SaveGrades(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<StudentGradeEntry> entries)
    {
        var list = entries.ToList();
        _repository.SaveGradeRecords(
            classId,
            assessment,
            date,
            maxScore,
            list.Select(e => (e.StudentId, e.Score, e.Comment)));

        return BuildGradeSheet(classId, assessment, date);
    }
}
