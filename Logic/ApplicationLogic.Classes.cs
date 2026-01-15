using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public List<SchoolClass> GetClassesForTeacher(int teacherId)
    {
        return _repository.GetClassesForTeacher(teacherId);
    }

    public List<SchoolClass> GetAllClasses()
    {
        return _repository.GetAllClasses();
    }

    public SchoolClass AddClass(SchoolClass schoolClass)
    {
        return _repository.AddClass(schoolClass);
    }

    public SchoolClass? GetClassWithEnrollments(int classId, int? teacherId = null)
    {
        return _repository.GetClassWithEnrollments(classId, teacherId);
    }

    public string? GetClassName(int classId)
    {
        return _repository.GetClassName(classId);
    }

    public int GetClassCount(int teacherId)
    {
        return _repository.GetClassCount(teacherId);
    }

    public int GetDistinctStudentCount(int teacherId)
    {
        return _repository.GetDistinctStudentCount(teacherId);
    }

    public OperationResult<SchoolClass> CreateClass(int teacherId, string name, string? room, string? description)
    {
        string trimmedName;
        if (name == null)
        {
            trimmedName = string.Empty;
        }
        else
        {
            trimmedName = name.Trim();
        }
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

    public Student? GetStudentByEmail(string email)
    {
        return _repository.GetStudentByEmail(email);
    }

    public Student AddStudent(Student student)
    {
        return _repository.AddStudent(student);
    }

    public bool EnrollmentExists(int studentId, int classId)
    {
        return _repository.EnrollmentExists(studentId, classId);
    }

    public void AddEnrollment(int studentId, int classId)
    {
        _repository.AddEnrollment(studentId, classId);
    }

    public ClassEnrollment? GetEnrollmentWithDetails(int enrollmentId, int teacherId)
    {
        return _repository.GetEnrollmentWithDetails(enrollmentId, teacherId);
    }

    public void RemoveEnrollment(int enrollmentId)
    {
        _repository.RemoveEnrollment(enrollmentId);
    }

    public List<Student> GetStudentsForClass(int classId)
    {
        return _repository.GetStudentsForClass(classId);
    }

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

        ClassOverlay? loadedOverlay = LoadClassOverlay(classId, teacherId).Value;
        ClassOverlay refreshedOverlay;
        if (loadedOverlay == null)
        {
            refreshedOverlay = overlayResult.Value;
        }
        else
        {
            refreshedOverlay = loadedOverlay;
        }
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

        ClassOverlay? loadedOverlay = LoadClassOverlay(classId, null).Value;
        ClassOverlay refreshedOverlay;
        if (loadedOverlay == null)
        {
            refreshedOverlay = overlayResult.Value;
        }
        else
        {
            refreshedOverlay = loadedOverlay;
        }
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
        ClassOverlay overlayResult;
        if (refreshedOverlay == null)
        {
            overlayResult = new ClassOverlay { ActiveClass = enrollment.SchoolClass };
        }
        else
        {
            overlayResult = refreshedOverlay;
        }

        return new ClassMembershipResult
        {
            Success = true,
            Message = message,
            Overlay = overlayResult,
            AlreadyEnrolled = false
        };
    }

    public List<SchoolClass> GetClassesForUserOrFallback(int userId)
    {
        List<SchoolClass> classes = _repository.GetClassesForTeacher(userId);
        if (classes.Count > 0)
        {
            return classes;
        }

        return _repository.GetAllClasses();
    }
}
