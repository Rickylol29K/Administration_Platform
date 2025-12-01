namespace Logic.Models;

public class AttendanceRoster
{
    public string ClassName { get; init; } = string.Empty;
    public List<StudentAttendance> Students { get; init; } = new();
}

public class StudentAttendance
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPresent { get; set; }
}
