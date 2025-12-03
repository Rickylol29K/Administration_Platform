using AdministrationPlat.Models;
using Logic.Models;

namespace Logic.Contracts;

public interface IGradeLogic
{
    List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date);
    void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records);
    List<GradeRecord> GetRecentGrades(int teacherId, int take);
    OperationResult<GradeSheet> BuildGradeSheet(int classId, string assessment, DateTime date);
    OperationResult<GradeSheet> SaveGrades(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<StudentGradeEntry> entries);
}
