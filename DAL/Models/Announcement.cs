using System;
using System.ComponentModel.DataAnnotations;

namespace AdministrationPlat.Models;

public class Announcement
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Body { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
