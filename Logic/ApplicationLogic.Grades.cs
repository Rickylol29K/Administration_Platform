using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date)
    {
        return _repository.GetGradeRecords(classId, assessment, date);
    }

    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records)
    {
        _repository.SaveGradeRecords(classId, assessment, date, maxScore, records);
    }

    public List<GradeRecord> GetRecentGrades(int teacherId, int take)
    {
        return _repository.GetRecentGrades(teacherId, take);
    }

    public OperationResult<GradeSheet> BuildGradeSheet(int classId, string assessment, DateTime date)
    {
        SchoolClass? classInfo = _repository.GetClassWithEnrollments(classId);

        if (classInfo == null)
        {
            return OperationResult<GradeSheet>.Fail("Class not found.");
        }

        List<Student> students = new List<Student>();
        foreach (ClassEnrollment enrollment in classInfo.Enrollments)
        {
            if (enrollment.Student != null)
            {
                students.Add(enrollment.Student);
            }
        }

        students.Sort((left, right) =>
        {
            int lastNameCompare = string.Compare(left.LastName, right.LastName, StringComparison.Ordinal);
            if (lastNameCompare != 0)
            {
                return lastNameCompare;
            }

            return string.Compare(left.FirstName, right.FirstName, StringComparison.Ordinal);
        });

        List<GradeRecord> existingRecords = _repository.GetGradeRecords(classId, assessment, date);
        Dictionary<int, GradeRecord> existing = new Dictionary<int, GradeRecord>();
        foreach (GradeRecord record in existingRecords)
        {
            existing[record.StudentId] = record;
        }

        List<StudentGradeEntry> entries = new List<StudentGradeEntry>();
        foreach (Student student in students)
        {
            GradeRecord? record = null;
            existing.TryGetValue(student.Id, out record);

            StudentGradeEntry entry = new StudentGradeEntry
            {
                StudentId = student.Id,
                StudentName = (student.FirstName + " " + student.LastName).Trim(),
                Score = record == null ? null : record.Score,
                Comment = record == null ? null : record.Comments
            };
            entries.Add(entry);
        }

        GradeSheet sheet = new GradeSheet
        {
            ClassName = classInfo.Name,
            Entries = entries
        };

        return OperationResult<GradeSheet>.Ok(sheet);
    }

    public OperationResult<GradeSheet> SaveGrades(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<StudentGradeEntry> entries)
    {
        List<StudentGradeEntry> list = entries.ToList();
        List<(int StudentId, decimal? Score, string? Comment)> tuples = new List<(int StudentId, decimal? Score, string? Comment)>();
        foreach (StudentGradeEntry entry in list)
        {
            tuples.Add((entry.StudentId, entry.Score, entry.Comment));
        }

        _repository.SaveGradeRecords(classId, assessment, date, maxScore, tuples);

        return BuildGradeSheet(classId, assessment, date);
    }
}
