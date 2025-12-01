using AdministrationPlat.Models;

namespace Logic.Models;

public class ClassOverlay
{
    public SchoolClass? ActiveClass { get; init; }
    public List<ClassEnrollment> Enrollments { get; init; } = new();
}

public class ClassMembershipResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public ClassOverlay Overlay { get; init; } = new();
    public bool AlreadyEnrolled { get; init; }
}
