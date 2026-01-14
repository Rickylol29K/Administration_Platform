using AdministrationPlat.Models;
using DAL;
using Logic.Models;

namespace Logic;

public class ApplicationLogic : ILogicService
{
    private readonly IDataRepository _repository;

    // Application logic.
    public ApplicationLogic(IDataRepository repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        _repository = repository;
    }

    // Login.
    public OperationResult<User> Login(string username, string password)
    {
        string trimmedUsername = (username ?? string.Empty).Trim();
        string trimmedPassword = (password ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(trimmedPassword))
        {
            return OperationResult<User>.Fail("Enter both username and password.");
        }

        User? user = _repository.GetUser(trimmedUsername, trimmedPassword);
        if (user == null)
        {
            return OperationResult<User>.Fail("Invalid username or password.");
        }

        return OperationResult<User>.Ok(user);
    }

    // Register.
    public OperationResult<User> Register(string username, string password, bool isAdmin)
    {
        string trimmedUsername = (username ?? string.Empty).Trim();
        string trimmedPassword = (password ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(trimmedPassword))
        {
            return OperationResult<User>.Fail("Choose a username and password.");
        }

        if (_repository.UsernameExists(trimmedUsername))
        {
            return OperationResult<User>.Fail("Username already exists.");
        }

        User user = _repository.CreateUser(trimmedUsername, trimmedPassword, isAdmin);
        return OperationResult<User>.Ok(user);
    }

    // Get user.
    public User? GetUser(string username, string password)
    {
        return _repository.GetUser(username, password);
    }

    // Get user by id.
    public User? GetUserById(int id)
    {
        return _repository.GetUserById(id);
    }

    // Username exists.
    public bool UsernameExists(string username)
    {
        return _repository.UsernameExists(username);
    }

    // Create user.
    public User CreateUser(string username, string password, bool isAdmin)
    {
        return _repository.CreateUser(username, password, isAdmin);
    }

    // Get teachers.
    public List<User> GetTeachers()
    {
        return _repository.GetTeachers();
    }

    // Get classes for teacher.
    public List<SchoolClass> GetClassesForTeacher(int teacherId)
    {
        return _repository.GetClassesForTeacher(teacherId);
    }

    // Get all classes.
    public List<SchoolClass> GetAllClasses()
    {
        return _repository.GetAllClasses();
    }

    // Add class.
    public SchoolClass AddClass(SchoolClass schoolClass)
    {
        return _repository.AddClass(schoolClass);
    }

    // Get class with enrollments.
    public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null)
    {
        return _repository.GetClassWithEnrollments(classId, teacherId);
    }

    // Get class name.
    public string? GetClassName(int classId)
    {
        return _repository.GetClassName(classId);
    }

    // Get class count.
    public int GetClassCount(int teacherId)
    {
        return _repository.GetClassCount(teacherId);
    }

    // Get distinct student count.
    public int GetDistinctStudentCount(int teacherId)
    {
        return _repository.GetDistinctStudentCount(teacherId);
    }

    // Create class.
    public OperationResult<SchoolClass> CreateClass(int teacherId, string name, string? room, string? description)
    {
        string trimmedName = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return OperationResult<SchoolClass>.Fail("Class name is required.");
        }

        SchoolClass schoolClass = new SchoolClass
        {
            Name = trimmedName,
            Room = string.IsNullOrWhiteSpace(room) ? null : room.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            TeacherId = teacherId
        };

        SchoolClass created = _repository.AddClass(schoolClass);
        return OperationResult<SchoolClass>.Ok(created);
    }

    // Load class overlay.
    public OperationResult<ClassOverlay> LoadClassOverlay(int classId, int? teacherId = null)
    {
        SchoolClass? schoolClass = _repository.GetClassWithEnrollments(classId, teacherId);
        if (schoolClass == null)
        {
            return OperationResult<ClassOverlay>.Fail("Unable to load the requested class.");
        }

        List<ClassEnrollment> enrollments = new List<ClassEnrollment>();
        foreach (ClassEnrollment enrollment in schoolClass.Enrollments)
        {
            if (enrollment.Student == null)
            {
                continue;
            }

            enrollments.Add(enrollment);
        }

        enrollments.Sort((left, right) =>
        {
            int lastNameCompare = string.Compare(left.Student!.LastName, right.Student!.LastName, StringComparison.Ordinal);
            if (lastNameCompare != 0)
            {
                return lastNameCompare;
            }

            return string.Compare(left.Student!.FirstName, right.Student!.FirstName, StringComparison.Ordinal);
        });

        ClassOverlay overlay = new ClassOverlay
        {
            ActiveClass = schoolClass,
            Enrollments = enrollments
        };

        return OperationResult<ClassOverlay>.Ok(overlay);
    }

    // Get student by email.
    public Student? GetStudentByEmail(string email)
    {
        return _repository.GetStudentByEmail(email);
    }

    // Add student.
    public Student AddStudent(Student student)
    {
        return _repository.AddStudent(student);
    }

    // Enrollment exists.
    public bool EnrollmentExists(int studentId, int classId)
    {
        return _repository.EnrollmentExists(studentId, classId);
    }

    // Add enrollment.
    public void AddEnrollment(int studentId, int classId)
    {
        _repository.AddEnrollment(studentId, classId);
    }

    // Get enrollment with details.
    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId)
    {
        return _repository.GetEnrollmentWithDetails(enrollmentId, teacherId);
    }

    // Remove enrollment.
    public void RemoveEnrollment(int enrollmentId)
    {
        _repository.RemoveEnrollment(enrollmentId);
    }

    // Get students for class.
    public List<Student> GetStudentsForClass(int classId)
    {
        return _repository.GetStudentsForClass(classId);
    }

    // Add student to class.
    public ClassMembershipResult AddStudentToClass(int teacherId, int classId, string firstName, string lastName, string? email)
    {
        OperationResult<ClassOverlay> overlayResult = LoadClassOverlay(classId, teacherId);
        if (!overlayResult.Success || overlayResult.Value?.ActiveClass == null)
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Class not found."
            };
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Student first and last name are required.",
                Overlay = overlayResult.Value
            };
        }

        Student? student = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            student = _repository.GetStudentByEmail(email.Trim());
        }

        if (student == null)
        {
            student = new Student
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
            };
            _repository.AddStudent(student);
        }

        bool alreadyEnrolled = _repository.EnrollmentExists(student.Id, classId);
        if (!alreadyEnrolled)
        {
            _repository.AddEnrollment(student.Id, classId);
        }

        ClassOverlay refreshedOverlay = LoadClassOverlay(classId, teacherId).Value ?? overlayResult.Value;
        string message;
        if (alreadyEnrolled)
        {
            message = student.FullName + " is already enrolled in this class.";
        }
        else
        {
            message = student.FullName + " added to " + overlayResult.Value.ActiveClass?.Name + ".";
        }

        return new ClassMembershipResult
        {
            Success = true,
            Message = message,
            Overlay = refreshedOverlay,
            AlreadyEnrolled = alreadyEnrolled
        };
    }

    // Add student to class as admin.
    public ClassMembershipResult AddStudentToClassAsAdmin(int classId, string firstName, string lastName, string? email)
    {
        OperationResult<ClassOverlay> overlayResult = LoadClassOverlay(classId, null);
        if (!overlayResult.Success || overlayResult.Value?.ActiveClass == null)
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Class not found."
            };
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Student first and last name are required.",
                Overlay = overlayResult.Value
            };
        }

        Student? student = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            student = _repository.GetStudentByEmail(email.Trim());
        }

        if (student == null)
        {
            student = new Student
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
            };
            _repository.AddStudent(student);
        }

        bool alreadyEnrolled = _repository.EnrollmentExists(student.Id, classId);
        if (!alreadyEnrolled)
        {
            _repository.AddEnrollment(student.Id, classId);
        }

        ClassOverlay refreshedOverlay = LoadClassOverlay(classId, null).Value ?? overlayResult.Value;
        string message;
        if (alreadyEnrolled)
        {
            message = student.FullName + " is already enrolled in this class.";
        }
        else
        {
            message = student.FullName + " added to " + overlayResult.Value.ActiveClass?.Name + ".";
        }

        return new ClassMembershipResult
        {
            Success = true,
            Message = message,
            Overlay = refreshedOverlay,
            AlreadyEnrolled = alreadyEnrolled
        };
    }

    // Remove student from class.
    public ClassMembershipResult RemoveStudentFromClass(int teacherId, int enrollmentId)
    {
        ClassEnrollment? enrollment = _repository.GetEnrollmentWithDetails(enrollmentId, teacherId);

        if (enrollment == null)
        {
            return new ClassMembershipResult
            {
                Success = false,
                Message = "Enrollment not found."
            };
        }

        _repository.RemoveEnrollment(enrollmentId);

        ClassOverlay? refreshedOverlay = LoadClassOverlay(enrollment.SchoolClassId, teacherId).Value;
        string studentName = enrollment.Student == null ? "Student" : enrollment.Student.FullName;
        string className = enrollment.SchoolClass == null ? string.Empty : enrollment.SchoolClass.Name;
        string message = studentName + " removed from " + className + ".";

        return new ClassMembershipResult
        {
            Success = true,
            Message = message,
            Overlay = refreshedOverlay ?? new ClassOverlay { ActiveClass = enrollment.SchoolClass },
            AlreadyEnrolled = false
        };
    }

    // Get attendance records.
    public List<AttendanceRecord> GetAttendanceRecords(int classId, DateTime date)
    {
        return _repository.GetAttendanceRecords(classId, date);
    }

    // Save attendance records.
    public void SaveAttendanceRecords(int classId, DateTime date, IEnumerable<(int StudentId, bool IsPresent)> records)
    {
        _repository.SaveAttendanceRecords(classId, date, records);
    }

    // Get classes for user or fallback.
    public List<SchoolClass> GetClassesForUserOrFallback(int userId)
    {
        List<SchoolClass> classes = _repository.GetClassesForTeacher(userId);
        if (classes.Count > 0)
        {
            return classes;
        }

        return _repository.GetAllClasses();
    }

    // Build attendance roster.
    public OperationResult<AttendanceRoster> BuildAttendanceRoster(int classId, DateTime date)
    {
        string className = _repository.GetClassName(classId) ?? string.Empty;
        List<Student> roster = _repository.GetStudentsForClass(classId);
        List<AttendanceRecord> existingRecords = _repository.GetAttendanceRecords(classId, date);

        Dictionary<int, bool> existing = new Dictionary<int, bool>();
        foreach (AttendanceRecord record in existingRecords)
        {
            existing[record.StudentId] = record.IsPresent;
        }

        List<StudentAttendance> studentEntries = new List<StudentAttendance>();
        foreach (Student student in roster)
        {
            bool isPresent = false;
            bool hasRecord = existing.TryGetValue(student.Id, out bool storedStatus);
            if (hasRecord)
            {
                isPresent = storedStatus;
            }

            StudentAttendance entry = new StudentAttendance
            {
                StudentId = student.Id,
                StudentName = (student.FirstName + " " + student.LastName).Trim(),
                IsPresent = isPresent
            };
            studentEntries.Add(entry);
        }

        AttendanceRoster rosterModel = new AttendanceRoster
        {
            ClassName = className,
            Students = studentEntries
        };

        return OperationResult<AttendanceRoster>.Ok(rosterModel);
    }

    // Save attendance.
    public OperationResult<AttendanceRoster> SaveAttendance(int classId, DateTime date, IEnumerable<StudentAttendance> records)
    {
        List<StudentAttendance> items = records.ToList();
        List<(int StudentId, bool IsPresent)> tuples = new List<(int StudentId, bool IsPresent)>();
        foreach (StudentAttendance entry in items)
        {
            tuples.Add((entry.StudentId, entry.IsPresent));
        }

        _repository.SaveAttendanceRecords(classId, date, tuples);

        return BuildAttendanceRoster(classId, date);
    }

    // Get grade records.
    public List<GradeRecord> GetGradeRecords(int classId, string assessment, DateTime date)
    {
        return _repository.GetGradeRecords(classId, assessment, date);
    }

    // Save grade records.
    public void SaveGradeRecords(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<(int StudentId, decimal? Score, string? Comment)> records)
    {
        _repository.SaveGradeRecords(classId, assessment, date, maxScore, records);
    }

    // Get recent grades.
    public List<GradeRecord> GetRecentGrades(int teacherId, int take)
    {
        return _repository.GetRecentGrades(teacherId, take);
    }

    // Build grade sheet.
    public OperationResult<GradeSheet> BuildGradeSheet(int classId, string assessment, DateTime date)
    {
        SchoolClass? classInfo = _repository.GetClassWithEnrollments(classId);

        if (classInfo == null)
        {
            return OperationResult<GradeSheet>.Fail("Class not found.");
        }

        List<Student> students = new List<Student>();
        foreach (ClassEnrollment enrollment in classInfo.Enrollments)
        {
            if (enrollment.Student != null)
            {
                students.Add(enrollment.Student);
            }
        }

        students.Sort((left, right) =>
        {
            int lastNameCompare = string.Compare(left.LastName, right.LastName, StringComparison.Ordinal);
            if (lastNameCompare != 0)
            {
                return lastNameCompare;
            }

            return string.Compare(left.FirstName, right.FirstName, StringComparison.Ordinal);
        });

        List<GradeRecord> existingRecords = _repository.GetGradeRecords(classId, assessment, date);
        Dictionary<int, GradeRecord> existing = new Dictionary<int, GradeRecord>();
        foreach (GradeRecord record in existingRecords)
        {
            existing[record.StudentId] = record;
        }

        List<StudentGradeEntry> entries = new List<StudentGradeEntry>();
        foreach (Student student in students)
        {
            GradeRecord? record = null;
            existing.TryGetValue(student.Id, out record);

            StudentGradeEntry entry = new StudentGradeEntry
            {
                StudentId = student.Id,
                StudentName = (student.FirstName + " " + student.LastName).Trim(),
                Score = record == null ? null : record.Score,
                Comment = record == null ? null : record.Comments
            };
            entries.Add(entry);
        }

        GradeSheet sheet = new GradeSheet
        {
            ClassName = classInfo.Name,
            Entries = entries
        };

        return OperationResult<GradeSheet>.Ok(sheet);
    }

    // Save grades.
    public OperationResult<GradeSheet> SaveGrades(int classId, string assessment, DateTime date, decimal? maxScore, IEnumerable<StudentGradeEntry> entries)
    {
        List<StudentGradeEntry> list = entries.ToList();
        List<(int StudentId, decimal? Score, string? Comment)> tuples = new List<(int StudentId, decimal? Score, string? Comment)>();
        foreach (StudentGradeEntry entry in list)
        {
            tuples.Add((entry.StudentId, entry.Score, entry.Comment));
        }

        _repository.SaveGradeRecords(classId, assessment, date, maxScore, tuples);

        return BuildGradeSheet(classId, assessment, date);
    }

    // Get upcoming events.
    public List<EventItem> GetUpcomingEvents(int userId, DateTime today, int take)
    {
        return _repository.GetUpcomingEvents(userId, today, take);
    }

    // Get events for month.
    public List<EventItem> GetEventsForMonth(int userId, int year, int month)
    {
        return _repository.GetEventsForMonth(userId, year, month);
    }

    // Get event.
    public EventItem? GetEvent(Guid id, int userId)
    {
        return _repository.GetEvent(id, userId);
    }

    // Add event.
    public void AddEvent(EventItem item)
    {
        _repository.AddEvent(item);
    }

    // Update event.
    public void UpdateEvent(EventItem item)
    {
        _repository.UpdateEvent(item);
    }

    // Delete event.
    public void DeleteEvent(Guid id, int userId)
    {
        _repository.DeleteEvent(id, userId);
    }

    // Create event.
    public OperationResult<EventItem> CreateEvent(int userId, EventItem item)
    {
        string title = (item.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return OperationResult<EventItem>.Fail("Event title is required.");
        }

        EventItem normalized = new EventItem
        {
            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
            Title = title,
            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
            Location = string.IsNullOrWhiteSpace(item.Location) ? null : item.Location.Trim(),
            Time = string.IsNullOrWhiteSpace(item.Time) ? null : item.Time.Trim(),
            Day = item.Day,
            Month = item.Month,
            Year = item.Year,
            UserId = userId
        };

        _repository.AddEvent(normalized);
        return OperationResult<EventItem>.Ok(normalized);
    }

    // Update event details.
    public OperationResult<EventItem> UpdateEventDetails(int userId, EventItem item)
    {
        EventItem? existing = _repository.GetEvent(item.Id, userId);
        if (existing == null)
        {
            return OperationResult<EventItem>.Fail("Event not found.");
        }

        string title = (item.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return OperationResult<EventItem>.Fail("Event title is required.");
        }

        existing.Title = title;
        existing.Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim();
        existing.Location = string.IsNullOrWhiteSpace(item.Location) ? null : item.Location.Trim();
        existing.Time = string.IsNullOrWhiteSpace(item.Time) ? null : item.Time.Trim();

        _repository.UpdateEvent(existing);
        return OperationResult<EventItem>.Ok(existing);
    }

    // Delete event for user.
    public OperationResult<bool> DeleteEventForUser(int userId, Guid id)
    {
        EventItem? existing = _repository.GetEvent(id, userId);
        if (existing == null)
        {
            return OperationResult<bool>.Fail("Event not found.");
        }

        _repository.DeleteEvent(id, userId);
        return OperationResult<bool>.Ok(true);
    }

    // Build current month.
    public CalendarData BuildCurrentMonth()
    {
        DateTime today = DateTime.Today;
        return BuildMonth(today.Year, today.Month);
    }

    // Build calendar view.
    public CalendarView BuildCalendarView(int userId, int selectedDay)
    {
        DateTime today = DateTime.Today;
        return BuildCalendarView(userId, today.Year, today.Month, selectedDay);
    }

    // Build calendar view.
    public CalendarView BuildCalendarView(int userId, int year, int month, int selectedDay)
    {
        CalendarData calendar = BuildMonth(year, month);
        List<EventItem> monthEvents = _repository.GetEventsForMonth(userId, calendar.Year, calendar.Month);

        int safeSelectedDay = Math.Clamp(selectedDay, 1, calendar.Days.Count);
        List<EventItem> selectedEvents = new List<EventItem>();
        foreach (EventItem item in monthEvents)
        {
            if (item.Day == safeSelectedDay)
            {
                selectedEvents.Add(item);
            }
        }

        selectedEvents.Sort((left, right) => string.Compare(left.Time, right.Time, StringComparison.Ordinal));

        return new CalendarView
        {
            Calendar = calendar,
            MonthEvents = monthEvents,
            SelectedDayEvents = selectedEvents
        };
    }

    // Get announcements.
    public List<Announcement> GetAnnouncements(int take)
    {
        return _repository.GetAnnouncements(take);
    }

    // Get all announcements.
    public List<Announcement> GetAllAnnouncements()
    {
        return _repository.GetAllAnnouncements();
    }

    // Create announcement.
    public OperationResult<Announcement> CreateAnnouncement(int createdByUserId, string title, string? body)
    {
        string trimmedTitle = (title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            return OperationResult<Announcement>.Fail("Announcement title is required.");
        }

        Announcement announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = trimmedTitle,
            Body = string.IsNullOrWhiteSpace(body) ? null : body.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _repository.AddAnnouncement(announcement);
        return OperationResult<Announcement>.Ok(announcement);
    }

    // Delete announcement.
    public OperationResult<bool> DeleteAnnouncement(Guid id)
    {
        if (id == Guid.Empty)
        {
            return OperationResult<bool>.Fail("Announcement not found.");
        }

        Announcement? existing = _repository.GetAnnouncement(id);
        if (existing == null)
        {
            return OperationResult<bool>.Fail("Announcement not found.");
        }

        _repository.DeleteAnnouncement(id);
        return OperationResult<bool>.Ok(true);
    }

    // Build month.
    private static CalendarData BuildMonth(int year, int month)
    {
        int safeMonth = Math.Clamp(month, 1, 12);
        int daysInMonth = DateTime.DaysInMonth(year, safeMonth);
        DateTime date = new DateTime(year, safeMonth, 1);

        List<int> days = new List<int>();
        for (int day = 1; day <= daysInMonth; day++)
        {
            days.Add(day);
        }

        return new CalendarData
        {
            Year = year,
            Month = safeMonth,
            MonthName = date.ToString("MMMM"),
            Days = days
        };
    }
}
