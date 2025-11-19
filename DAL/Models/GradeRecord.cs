using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrationPlat.Models;

public class GradeRecord
{
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    public Student? Student { get; set; }

    [Required]
    public int SchoolClassId { get; set; }

    public SchoolClass? SchoolClass { get; set; }

    [Required]
    [MaxLength(100)]
    public string Assessment { get; set; } = string.Empty;

    public decimal? Score { get; set; }

    public decimal? MaxScore { get; set; }

    public DateTime DateRecorded { get; set; } = DateTime.UtcNow.Date;

    [MaxLength(500)]
    public string? Comments { get; set; }
}
