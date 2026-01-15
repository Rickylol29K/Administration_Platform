using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date)
    {
        return _repository.GetAttendanceRecords(classId, date);
    }

    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records)
    {
        _repository.SaveAttendanceRecords(classId, date, records);
    }

    public OperationResult<AttendanceRoster> BuildAttendanceRoster(int classId, DateTime date)
    {
        string? storedClassName = _repository.GetClassName(classId);
        string className;
        if (storedClassName == null)
        {
            className = string.Empty;
        }
        else
        {
            className = storedClassName;
        }
        List<Student> roster = _repository.GetStudentsForClass(classId);
        List<AttendanceRecord> existingRecords = _repository.GetAttendanceRecords(classId, date);

        Dictionary<int, bool> existing = new Dictionary<int, bool>();
        foreach (AttendanceRecord record in existingRecords)
        {
            existing[record.StudentId] = record.IsPresent;
        }

        List<StudentAttendance> studentEntries = new List<StudentAttendance>();
        foreach (Student student in roster)
        {
            bool isPresent = false;
            bool hasRecord = existing.TryGetValue(student.Id, out bool storedStatus);
            if (hasRecord)
            {
                isPresent = storedStatus;
            }

            StudentAttendance entry = new StudentAttendance
            {
                StudentId = student.Id,
                StudentName = (student.FirstName + " " + student.LastName).Trim(),
                IsPresent = isPresent
            };
            studentEntries.Add(entry);
        }

        AttendanceRoster rosterModel = new AttendanceRoster
        {
            ClassName = className,
            Students = studentEntries
        };

        return OperationResult<AttendanceRoster>.Ok(rosterModel);
    }

    public OperationResult<AttendanceRoster> SaveAttendance(int classId, DateTime date, IEnumerable<StudentAttendance> records)
    {
        List<StudentAttendance> items = records.ToList();
        List<(int StudentId, bool IsPresent)> tuples = new List<(int StudentId, bool IsPresent)>();
        foreach (StudentAttendance entry in items)
        {
            tuples.Add((entry.StudentId, entry.IsPresent));
        }

        _repository.SaveAttendanceRecords(classId, date, tuples);

        return BuildAttendanceRoster(classId, date);
    }
}
