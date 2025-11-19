using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrationPlat.Models;

public class EventItem
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(120)]
    public string? Location { get; set; }

    [MaxLength(20)]
    public string? Time { get; set; }

    public int Day { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int UserId { get; set; }
}
