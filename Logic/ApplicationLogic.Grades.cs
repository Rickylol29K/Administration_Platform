using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date) =>
        _grades.GetGradeRecords(classId, assessment, date);

    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records) =>
        _grades.SaveGradeRecords(classId, assessment, date, maxScore, records);

    public List<GradeRecord> GetRecentGrades(int teacherId, int take) => _grades.GetRecentGrades(teacherId, take);

    public OperationResult<GradeSheet> BuildGradeSheet(int classId, string assessment, DateTime date) =>
        _grades.BuildGradeSheet(classId, assessment, date);

    public OperationResult<GradeSheet> SaveGrades(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<StudentGradeEntry> entries) =>
        _grades.SaveGrades(classId, assessment, date, maxScore, entries);
}
