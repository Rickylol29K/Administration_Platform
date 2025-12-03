using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date) => _attendance.GetAttendanceRecords(classId, date);

    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records) =>
        _attendance.SaveAttendanceRecords(classId, date, records);

    public List<SchoolClass> GetClassesForUserOrFallback(int userId) => _attendance.GetClassesForUserOrFallback(userId);

    public OperationResult<AttendanceRoster> BuildAttendanceRoster(int classId, DateTime date) =>
        _attendance.BuildAttendanceRoster(classId, date);

    public OperationResult<AttendanceRoster> SaveAttendance(int classId, DateTime date, IEnumerable<StudentAttendance> records) =>
        _attendance.SaveAttendance(classId, date, records);
}
