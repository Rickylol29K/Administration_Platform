using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdministrationPlat.Models;

public class SchoolClass
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Room { get; set; }

    [MaxLength(250)]
    public string? Description { get; set; }

    public int TeacherId { get; set; }

    public User? Teacher { get; set; }

    public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();
}
