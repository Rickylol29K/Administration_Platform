using AdministrationPlat.Models;
using Logic.Models;

namespace Logic.Contracts;

public interface IAttendanceLogic
{
    List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date);
    void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records);
    List<SchoolClass> GetClassesForUserOrFallback(int userId);
    OperationResult<AttendanceRoster> BuildAttendanceRoster(int classId, DateTime date);
    OperationResult<AttendanceRoster> SaveAttendance(int classId, DateTime date, IEnumerable<StudentAttendance> records);
}
