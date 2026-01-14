using AdministrationPlat.Models;
using DAL;

namespace AdministrationPlatformTesting.Infrastructure;

internal sealed class FakeDataRepository : IDataRepository
{
    private int _nextUserId = 1;
    private int _nextClassId = 1;
    private int _nextStudentId = 1;
    private int _nextEnrollmentId = 1;
    private int _nextAttendanceId = 1;
    private int _nextGradeId = 1;

    public List<User> Users { get; } = new();
    public List<SchoolClass> Classes { get; } = new();
    public List<Student> Students { get; } = new();
    public List<ClassEnrollment> Enrollments { get; } = new();
    public List<AttendanceRecord> AttendanceRecords { get; } = new();
    public List<GradeRecord> GradeRecords { get; } = new();
    public List<EventItem> Events { get; } = new();
    public List<Announcement> Announcements { get; } = new();

    public User? GetUser(string username, string password)
    {
        return Users.FirstOrDefault(u => u.Username == username && u.Password == password);
    }

    public User? GetUserById(int id)
    {
        return Users.FirstOrDefault(u => u.Id == id);
    }

    public bool UsernameExists(string username)
    {
        return Users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public User CreateUser(string username, string password, bool isAdmin)
    {
        var user = new User
        {
            Id = _nextUserId++,
            Username = username,
            Password = password,
            IsAdmin = isAdmin
        };
        Users.Add(user);
        return user;
    }

    public List<User> GetTeachers()
    {
        return Users.Where(u => !u.IsAdmin).ToList();
    }

    public List<SchoolClass> GetClassesForTeacher(int teacherId)
    {
        return Classes.Where(c => c.TeacherId == teacherId).ToList();
    }

    public List<SchoolClass> GetAllClasses()
    {
        return Classes.ToList();
    }

    public SchoolClass AddClass(SchoolClass schoolClass)
    {
        schoolClass.Id = _nextClassId++;
        Classes.Add(schoolClass);
        return schoolClass;
    }

    public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null)
    {
        var cls = Classes.FirstOrDefault(c => c.Id == classId);
        if (cls == null || (teacherId.HasValue && cls.TeacherId != teacherId.Value))
        {
            return null;
        }

        var enrollments = Enrollments
            .Where(e => e.SchoolClassId == classId)
            .Select(e => new ClassEnrollment
            {
                Id = e.Id,
                StudentId = e.StudentId,
                SchoolClassId = e.SchoolClassId,
                Student = Students.FirstOrDefault(s => s.Id == e.StudentId),
                SchoolClass = cls
            })
            .ToList();

        return new SchoolClass
        {
            Id = cls.Id,
            Name = cls.Name,
            Room = cls.Room,
            Description = cls.Description,
            TeacherId = cls.TeacherId,
            Teacher = cls.Teacher,
            Enrollments = enrollments
        };
    }

    public string? GetClassName(int classId)
    {
        SchoolClass? cls = Classes.FirstOrDefault(c => c.Id == classId);
        return cls == null ? null : cls.Name;
    }

    public Student? GetStudentByEmail(string email)
    {
        return Students.FirstOrDefault(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public Student AddStudent(Student student)
    {
        student.Id = _nextStudentId++;
        Students.Add(student);
        return student;
    }

    public bool EnrollmentExists(int studentId, int classId)
    {
        return Enrollments.Any(e => e.StudentId == studentId && e.SchoolClassId == classId);
    }

    public void AddEnrollment(int studentId, int classId)
    {
        Enrollments.Add(new ClassEnrollment
        {
            Id = _nextEnrollmentId++,
            StudentId = studentId,
            SchoolClassId = classId
        });
    }

    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId)
    {
        var enrollment = Enrollments.FirstOrDefault(e => e.Id == enrollmentId);
        if (enrollment == null)
        {
            return null;
        }

        var cls = Classes.FirstOrDefault(c => c.Id == enrollment.SchoolClassId);
        if (cls == null || cls.TeacherId != teacherId)
        {
            return null;
        }

        return new ClassEnrollment
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            SchoolClassId = enrollment.SchoolClassId,
            Student = Students.FirstOrDefault(s => s.Id == enrollment.StudentId),
            SchoolClass = cls
        };
    }

    public void RemoveEnrollment(int enrollmentId)
    {
        Enrollments.RemoveAll(e => e.Id == enrollmentId);
    }

    public List<Student> GetStudentsForClass(int classId)
    {
        var studentIds = Enrollments
            .Where(e => e.SchoolClassId == classId)
            .Select(e => e.StudentId)
            .ToHashSet();

        return Students.Where(s => studentIds.Contains(s.Id)).ToList();
    }

    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date)
    {
        return AttendanceRecords.Where(r => r.SchoolClassId == classId && r.Date.Date == date.Date).ToList();
    }

    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records)
    {
        var items = records.ToList();
        var existing = AttendanceRecords
            .Where(r => r.SchoolClassId == classId && r.Date.Date == date.Date)
            .ToList();

        foreach (var record in existing.ToList())
        {
            var match = items.FirstOrDefault(r => r.StudentId == record.StudentId);
            if (match.StudentId == 0 && items.All(r => r.StudentId != record.StudentId))
            {
                AttendanceRecords.Remove(record);
                continue;
            }

            record.IsPresent = match.IsPresent;
        }

        foreach (var record in items)
        {
            if (existing.Any(r => r.StudentId == record.StudentId))
            {
                continue;
            }

            AttendanceRecords.Add(new AttendanceRecord
            {
                Id = _nextAttendanceId++,
                StudentId = record.StudentId,
                SchoolClassId = classId,
                Date = date.Date,
                IsPresent = record.IsPresent
            });
        }
    }

    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date)
    {
        return GradeRecords
            .Where(r => r.SchoolClassId == classId && r.Assessment == assessment && r.DateRecorded.Date == date.Date)
            .ToList();
    }

    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records)
    {
        var items = records.ToList();
        var existing = GradeRecords
            .Where(r => r.SchoolClassId == classId && r.Assessment == assessment && r.DateRecorded.Date == date.Date)
            .ToList();

        foreach (var grade in existing.ToList())
        {
            var incoming = items.FirstOrDefault(r => r.StudentId == grade.StudentId);
            if (incoming.StudentId == 0 && items.All(r => r.StudentId != grade.StudentId))
            {
                GradeRecords.Remove(grade);
                continue;
            }

            if (!incoming.Score.HasValue)
            {
                GradeRecords.Remove(grade);
                continue;
            }

            grade.Score = incoming.Score;
            grade.MaxScore = maxScore;
            grade.Comments = string.IsNullOrWhiteSpace(incoming.Comment) ? null : incoming.Comment.Trim();
        }

        foreach (var incoming in items)
        {
            if (!incoming.Score.HasValue || existing.Any(r => r.StudentId == incoming.StudentId))
            {
                continue;
            }

            GradeRecords.Add(new GradeRecord
            {
                Id = _nextGradeId++,
                StudentId = incoming.StudentId,
                SchoolClassId = classId,
                Assessment = assessment,
                DateRecorded = date.Date,
                Score = incoming.Score,
                MaxScore = maxScore,
                Comments = string.IsNullOrWhiteSpace(incoming.Comment) ? null : incoming.Comment.Trim()
            });
        }
    }

    public List<GradeRecord> GetRecentGrades(int teacherId, int take)
    {
        var classIds = Classes.Where(c => c.TeacherId == teacherId).Select(c => c.Id).ToHashSet();

        return GradeRecords
            .Where(g => classIds.Contains(g.SchoolClassId))
            .OrderByDescending(g => g.DateRecorded)
            .ThenByDescending(g => g.Id)
            .Take(take)
            .ToList();
    }

    public int GetClassCount(int teacherId)
    {
        return Classes.Count(c => c.TeacherId == teacherId);
    }

    public int GetDistinctStudentCount(int teacherId)
    {
        var classIds = Classes.Where(c => c.TeacherId == teacherId).Select(c => c.Id).ToHashSet();
        return Enrollments
            .Where(e => classIds.Contains(e.SchoolClassId))
            .Select(e => e.StudentId)
            .Distinct()
            .Count();
    }

    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take)
    {
        var eventsWithDates = Events
            .Where(e => e.UserId == userId)
            .Select(e => new { Event = e, Date = SafeBuildDate(e) })
            .Where(e => e.Date.HasValue)
            .Select(e => new { e.Event, Date = e.Date!.Value })
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Event.Time, StringComparer.Ordinal)
            .Take(take)
            .Select(e => e.Event)
            .ToList();

        return eventsWithDates;
    }

    public List<EventItem> GetEventsForMonth(int userId, int year, int month)
    {
        return Events
            .Where(e => e.UserId == userId && e.Year == year && e.Month == month)
            .OrderBy(e => e.Day)
            .ThenBy(e => e.Time, StringComparer.Ordinal)
            .ToList();
    }

    public EventItem? GetEvent(Guid id, int userId)
    {
        return Events.FirstOrDefault(e => e.Id == id && e.UserId == userId);
    }

    public void AddEvent(EventItem item)
    {
        Events.Add(item);
    }

    public void UpdateEvent(EventItem item)
    {
        var existing = Events.FirstOrDefault(e => e.Id == item.Id && e.UserId == item.UserId);
        if (existing == null)
        {
            return;
        }

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Location = item.Location;
        existing.Time = item.Time;
        existing.Day = item.Day;
        existing.Month = item.Month;
        existing.Year = item.Year;
    }

    public void DeleteEvent(Guid id, int userId)
    {
        Events.RemoveAll(e => e.Id == id && e.UserId == userId);
    }

    public List<Announcement> GetAnnouncements(int take)
    {
        return Announcements
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToList();
    }

    public List<Announcement> GetAllAnnouncements()
    {
        return Announcements.OrderByDescending(a => a.CreatedAt).ToList();
    }

    public Announcement? GetAnnouncement(Guid id)
    {
        return Announcements.FirstOrDefault(a => a.Id == id);
    }

    public Announcement AddAnnouncement(Announcement announcement)
    {
        Announcements.Add(announcement);
        return announcement;
    }

    public void DeleteAnnouncement(Guid id)
    {
        Announcements.RemoveAll(a => a.Id == id);
    }

    private static DateTime? SafeBuildDate(EventItem e)
    {
        try
        {
            return new DateTime(e.Year, e.Month, e.Day);
        }
        catch
        {
            return null;
        }
    }
}
