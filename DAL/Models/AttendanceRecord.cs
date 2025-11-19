using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrationPlat.Models;

public class AttendanceRecord
{
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    public Student? Student { get; set; }

    [Required]
    public int SchoolClassId { get; set; }

    public SchoolClass? SchoolClass { get; set; }

    public DateTime Date { get; set; }

    public bool IsPresent { get; set; }
}
