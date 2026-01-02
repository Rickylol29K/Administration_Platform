using AdministrationPlat.Models;
using Microsoft.EntityFrameworkCore;

namespace AdministrationPlat.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<ClassEnrollment> Enrollments => Set<ClassEnrollment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<GradeRecord> GradeRecords => Set<GradeRecord>();
    public DbSet<EventItem> TeacherEvents => Set<EventItem>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .ToTable("Users")
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.IsAdmin)
            .HasDefaultValue(false);

        modelBuilder.Entity<SchoolClass>()
            .ToTable("Classes")
            .HasOne(c => c.Teacher)
            .WithMany()
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .ToTable("Students");

        modelBuilder.Entity<ClassEnrollment>()
            .ToTable("Enrollments");

        modelBuilder.Entity<AttendanceRecord>()
            .ToTable("AttendanceRecords");

        modelBuilder.Entity<GradeRecord>()
            .ToTable("GradeRecords");

        modelBuilder.Entity<EventItem>()
            .ToTable("TeacherEvents");

        modelBuilder.Entity<Announcement>()
            .ToTable("Announcements");

        modelBuilder.Entity<ClassEnrollment>()
            .HasIndex(e => new { e.StudentId, e.SchoolClassId })
            .IsUnique();

        modelBuilder.Entity<ClassEnrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClassEnrollment>()
            .HasOne(e => e.SchoolClass)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.SchoolClassId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(r => new { r.StudentId, r.SchoolClassId, r.Date })
            .IsUnique();

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(r => r.Student)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(r => r.SchoolClass)
            .WithMany()
            .HasForeignKey(r => r.SchoolClassId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GradeRecord>()
            .HasIndex(r => new { r.StudentId, r.SchoolClassId, r.Assessment, r.DateRecorded })
            .IsUnique();

        modelBuilder.Entity<GradeRecord>()
            .HasOne(r => r.Student)
            .WithMany(s => s.GradeRecords)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GradeRecord>()
            .HasOne(r => r.SchoolClass)
            .WithMany()
            .HasForeignKey(r => r.SchoolClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
