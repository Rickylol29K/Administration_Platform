# Unit Testing Plan

This document defines the full unit testing strategy for the Administration platform. The goal is to unit test every code path that contains business logic or state mutation, including the Razor PageModels, Logic services, ApplicationLogic facade, and model defaults. DAL SQL execution is treated separately because it depends on SQL Server and is not unit-testable without a database.

## Goals
- Verify correctness of business rules in Logic services.
- Validate PageModel behaviors (session checks, ModelState errors, redirects, TempData).
- Ensure model defaults and computed properties are stable.
- Cover edge cases (nulls, whitespace, missing IDs, empty lists).

## Non-Goals (Unit Test Scope)
- SQL execution inside `DAL/SqlDataRepository*` is integration-level and requires a real SQL Server. Those behaviors are documented here but should be validated with integration tests or a test database.
- UI rendering in `.cshtml` files is not unit tested; PageModel state and results are.

## Coverage Matrix (Unit Tests)

### Logic layer
- `Logic/Services/UserLogic.cs`
  - `Login`: empty inputs, invalid credentials, successful login.
  - `Register`: empty inputs, duplicate username, successful registration.
- `Logic/Services/ClassLogic.cs`
  - `CreateClass`: empty name, trimming and optional fields.
  - `LoadClassOverlay`: missing class, enrollment ordering.
  - `AddStudentToClass`: missing names, existing student by email, new student creation, already enrolled path.
  - `RemoveStudentFromClass`: missing enrollment, successful removal.
  - `GetClassCount`, `GetDistinctStudentCount`: counts for teacher.
- `Logic/Services/AttendanceLogic.cs`
  - `GetClassesForUserOrFallback`: returns teacher classes, fallback to all.
  - `BuildAttendanceRoster`: existing attendance values mapped to roster.
  - `SaveAttendance`: persists changes and returns updated roster.
- `Logic/Services/GradeLogic.cs`
  - `BuildGradeSheet`: missing class, enrollment ordering, existing grades mapping.
  - `SaveGrades`: persists scores and returns updated sheet.
- `Logic/Services/EventLogic.cs`
  - `CreateEvent`: required title, trimming, user ownership set.
  - `UpdateEventDetails`: missing event, empty title, field updates.
  - `DeleteEventForUser`: missing event, successful delete.
  - `BuildCurrentMonth`: correct month and day list length.
  - `BuildCalendarView`: selected day filtering.
- `Logic/ApplicationLogic*.cs`
  - Constructor null guard.
  - Delegation methods return expected values from services.

### Razor PageModels
- `AdministrationPlat/Pages/Index.cshtml.cs`
  - Login success sets session and redirects.
  - Login failure sets error message and css class.
  - Register success sets status message.
  - Register failure sets error message.
- `AdministrationPlat/Pages/Teacher/TeacherIndex.cshtml.cs`
  - Redirect when no session.
  - Loads counts/events/grades for logged-in teacher.
- `AdministrationPlat/Pages/Teacher/Classes.cshtml.cs`
  - Redirect when no session.
  - Add class validation and success path.
  - Show overlay loads class and enrollments.
  - Add student failure and success paths.
  - Remove student success and failure paths.
- `AdministrationPlat/Pages/Teacher/Attendance.cshtml.cs`
  - Redirect when no session.
  - Save attendance validation and success path.
- `AdministrationPlat/Pages/Teacher/Grading.cshtml.cs`
  - Redirect when no session.
  - Save grades validation and success path.
- `AdministrationPlat/Pages/Teacher/Calendar.cshtml.cs`
  - Redirect when no session.
  - Add event validation path.
  - Update event validation path.
  - Edit event populates form.
- `AdministrationPlat/Pages/Error.cshtml.cs`
  - `OnGet` populates `RequestId` and `ShowRequestId`.
- `AdministrationPlat/Pages/Privacy.cshtml.cs` and `AdministrationPlat/Pages/Admin/AdminIndex.cshtml.cs`
  - `OnGet` runs without errors.

### Models
- `DAL/Models/*`
  - Default list initialization and basic computed properties (`Student.FullName`).
- `Logic/Models/*`
  - Default list initialization and default values.
- `Logic/Models/OperationResult.cs`
  - `Ok` and `Fail` factory methods.

## DAL (Integration Test Plan)
For `DAL/SqlDataRepository*` methods, create integration tests against SQL Server:
- Verify insert/update/delete logic for Users, Classes, Students/Enrollments, Attendance, Grades, and Events.
- Validate foreign key constraints and uniqueness constraints.
- Verify query filters by teacher and class.

These are intentionally not unit tests and require a disposable test database.

## Test Data Strategy
- In-memory repository mimics the repository contract without SQL.
- Builders/helpers create students, classes, enrollments, and events with explicit IDs.
- Deterministic ordering ensures stable assertions.

## Naming & Structure
- Test project: `AdministrationPlatformTesting`.
- Test fixtures grouped by layer:
  - `Logic.*Tests`
  - `Pages.*Tests`
  - `Models.*Tests`
  - `Infrastructure` helpers (fake repository, test session).

## Execution
- Run with `dotnet test`.
- Coverage is collected via `coverlet.collector`.
