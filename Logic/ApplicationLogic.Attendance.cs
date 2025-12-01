using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date) => _repository.GetAttendanceRecords(classId, date);

    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records) =>
        _repository.SaveAttendanceRecords(classId, date, records);

    public List<SchoolClass> GetClassesForUserOrFallback(int userId)
    {
        var classes = _repository.GetClassesForTeacher(userId);
        return classes.Count > 0 ? classes : _repository.GetAllClasses();
    }

    public OperationResult<AttendanceRoster> BuildAttendanceRoster(int classId, DateTime date)
    {
        var className = _repository.GetClassName(classId) ?? string.Empty;
        var roster = _repository.GetStudentsForClass(classId);
        var existing = _repository.GetAttendanceRecords(classId, date)
            .ToDictionary(r => r.StudentId, r => r.IsPresent);

        var rosterModel = new AttendanceRoster
        {
            ClassName = className,
            Students = roster
                .Select(student => new StudentAttendance
                {
                    StudentId = student.Id,
                    StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                    IsPresent = existing.TryGetValue(student.Id, out var status) && status
                })
                .ToList()
        };

        return OperationResult<AttendanceRoster>.Ok(rosterModel);
    }

    public OperationResult<AttendanceRoster> SaveAttendance(int classId, DateTime date, IEnumerable<StudentAttendance> records)
    {
        var items = records.ToList();
        var tuples = items.Select(r => (r.StudentId, r.IsPresent));

        _repository.SaveAttendanceRecords(classId, date, tuples);

        return BuildAttendanceRoster(classId, date);
    }
}
