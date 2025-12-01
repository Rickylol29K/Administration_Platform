namespace Logic.Models;

public class GradeSheet
{
    public string ClassName { get; init; } = string.Empty;
    public List<StudentGradeEntry> Entries { get; init; } = new();
}

public class StudentGradeEntry
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
}
