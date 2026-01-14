using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdministrationPlat.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [NotMapped]
    public string FullName
    {
        get
        {
            return $"{FirstName} {LastName}".Trim();
        }
    }

    public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public ICollection<GradeRecord> GradeRecords { get; set; } = new List<GradeRecord>();
}
