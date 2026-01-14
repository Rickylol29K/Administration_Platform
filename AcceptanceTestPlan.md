# Acceptance Testing Plan - Administration Platform

## 1) Purpose
Validate that the Administration Platform meets the expected behaviors for Admin and Teacher users across core workflows: authentication, class management, student enrollment, announcements, attendance, grading, and calendar events.

## 2) Scope
### In scope
- Role-based access and session handling
- Admin workflows: teacher account creation, class creation, student enrollment, announcements
- Teacher workflows: view classes, attendance, grading, calendar events
- Page-level validations, error states, and basic navigation
- Data persistence for core entities

### Out of scope
- Performance/load testing beyond basic responsiveness checks
- Security penetration testing
- Accessibility audits beyond basic UX checks
- Deployment automation and CI/CD validation

## 3) References
- `ResearchDocument.md`
- `AdministrationPlatformTesting/` (existing automated tests)
- `AdministrationPlat/Pages/` (UI pages)
- `Logic/` and `DAL/` (business and data access layers)

## 4) Test Environment
### Hardware/OS
- Local developer workstation (macOS or Windows)

### Software
- .NET SDK per `global.json`
- SQL Server (local or container), configured in `AdministrationPlat/appsettings.json`
- Browser: Chrome or Edge (latest)

### Environment configuration
- Ensure database is reachable and schema created (migrations or setup scripts as used by the project).
- Seed data for Admin and Teacher roles (see Section 6).

## 5) Roles and Access
### Admin
- Can create teacher accounts
- Can create classes
- Can add students to classes
- Can create announcements

### Teacher
- Can view their classes
- Can record attendance by date
- Can enter grades
- Can manage calendar events

## 6) Test Data
### Baseline users
- Admin user: `admin@test.local` / `Admin!234`
- Teacher user: `teacher1@test.local` / `Teacher!234`

### Classes
- Class A: "Math 101"
- Class B: "History 201"

### Students
- Student 1: "Alex Johnson"
- Student 2: "Sam Rivera"
- Student 3: "Casey Lee"

### Events
- Event 1: "Parent Meeting" on next Monday, 10:00-11:00
- Event 2: "Quiz Day" on next Wednesday, 08:00-09:00

### Announcements
- Announcement 1: "Welcome to the new term."
- Announcement 2: "Schedule updated for next week."

## 7) Entry Criteria
- Application builds and launches without errors.
- Database is available and seeded with required test data.
- Testers have credentials for Admin and Teacher roles.

## 8) Exit Criteria
- All acceptance test cases executed.
- No open high-severity defects.
- Any medium/low defects logged and triaged.

## 9) Acceptance Test Cases
The following cases are written to be executed as manual, end-to-end validations. Each case includes expected results.

### Authentication and Session
#### AT-AUTH-01: Login with valid Admin credentials
- Steps:
  1. Navigate to the Login page.
  2. Enter Admin credentials.
  3. Submit.
- Expected:
  - Redirect to Admin landing page.
  - Admin navigation visible (Admin index links).

#### AT-AUTH-02: Login with valid Teacher credentials
- Steps:
  1. Navigate to Login page.
  2. Enter Teacher credentials.
  3. Submit.
- Expected:
  - Redirect to Teacher landing page.
  - Teacher navigation visible (Classes, Attendance, Grading, Calendar).

#### AT-AUTH-03: Login with invalid credentials
- Steps:
  1. Navigate to Login page.
  2. Enter invalid username/password.
  3. Submit.
- Expected:
  - Error message shown.
  - No navigation to Admin/Teacher pages.

#### AT-AUTH-04: Session persistence
- Steps:
  1. Login as Admin.
  2. Navigate across pages.
  3. Refresh browser.
- Expected:
  - Session persists.
  - User remains logged in.

#### AT-AUTH-05: Unauthorized access blocked
- Steps:
  1. Logout or use a private session.
  2. Directly access a Teacher page URL.
- Expected:
  - Redirect to Login or access denied.

### Admin - Teacher Account Management
#### AT-ADM-01: Create a new teacher account
- Steps:
  1. Login as Admin.
  2. Open Teacher creation form.
  3. Enter new teacher details.
  4. Submit.
- Expected:
  - Success message.
  - Teacher appears in teacher list.

#### AT-ADM-02: Validation on teacher creation
- Steps:
  1. Leave required fields empty.
  2. Submit.
- Expected:
  - Inline validation errors.
  - No user created.

### Admin - Class Management
#### AT-ADM-03: Create a new class
- Steps:
  1. Login as Admin.
  2. Open class creation form.
  3. Enter class name and teacher assignment.
  4. Submit.
- Expected:
  - Class appears in class list.
  - Class is visible to assigned teacher.

#### AT-ADM-04: Create class with missing fields
- Steps:
  1. Leave class name empty.
  2. Submit.
- Expected:
  - Validation error.
  - No class created.

### Admin - Student Enrollment
#### AT-ADM-05: Add students to class
- Steps:
  1. Login as Admin.
  2. Select a class.
  3. Add Student 1 and Student 2.
  4. Submit.
- Expected:
  - Students appear in class roster.
  - Teacher can view roster.

#### AT-ADM-06: Prevent duplicate enrollment
- Steps:
  1. Add Student 1 to a class.
  2. Attempt to add the same student again.
- Expected:
  - Duplicate enrollment is blocked or ignored with a message.

### Admin - Announcements
#### AT-ADM-07: Create announcement
- Steps:
  1. Login as Admin.
  2. Create an announcement with message and date.
  3. Submit.
- Expected:
  - Announcement visible on Admin landing page.
  - Announcement visible on Teacher landing page (if shared).

#### AT-ADM-08: Announcement validation
- Steps:
  1. Submit announcement with empty message.
- Expected:
  - Validation error.
  - No announcement created.

### Teacher - Classes
#### AT-TEACH-01: View classes assigned to teacher
- Steps:
  1. Login as Teacher.
  2. Open Classes page.
- Expected:
  - Assigned classes listed.
  - Each class shows roster count.

#### AT-TEACH-02: View class roster
- Steps:
  1. Select "Math 101".
  2. View roster.
- Expected:
  - Roster includes enrolled students.

### Teacher - Attendance
#### AT-TEACH-03: Record attendance for a class date
- Steps:
  1. Login as Teacher.
  2. Open Attendance page for "Math 101".
  3. Select date (today).
  4. Mark Student 1 present, Student 2 absent.
  5. Submit.
- Expected:
  - Attendance saved.
  - Confirmation message shown.

#### AT-TEACH-04: View attendance history
- Steps:
  1. After AT-TEACH-03, open Attendance history.
- Expected:
  - Entries show correct status for the date.

#### AT-TEACH-05: Attendance validation
- Steps:
  1. Submit attendance without selecting a date.
- Expected:
  - Validation error.
  - No attendance created.

### Teacher - Grading
#### AT-TEACH-06: Create grade sheet
- Steps:
  1. Login as Teacher.
  2. Open Grading page for "Math 101".
  3. Create new grade sheet.
- Expected:
  - Grade sheet created and listed.

#### AT-TEACH-07: Enter grades
- Steps:
  1. Open grade sheet.
  2. Enter grades for Student 1 and Student 2.
  3. Save.
- Expected:
  - Grades saved and visible after refresh.

#### AT-TEACH-08: Grade validation
- Steps:
  1. Enter invalid score (e.g., out of range or non-numeric).
  2. Save.
- Expected:
  - Validation error and no save.

### Teacher - Calendar Events
#### AT-TEACH-09: Create calendar event
- Steps:
  1. Login as Teacher.
  2. Open Calendar page.
  3. Create Event 1.
- Expected:
  - Event appears in calendar list.

#### AT-TEACH-10: Edit calendar event
- Steps:
  1. Edit Event 1 title or time.
  2. Save.
- Expected:
  - Updated event displayed.

#### AT-TEACH-11: Delete calendar event
- Steps:
  1. Delete Event 1.
- Expected:
  - Event removed from calendar list.

### Cross-Role Visibility
#### AT-CROSS-01: Admin-created classes visible to Teacher
- Steps:
  1. Admin creates "History 201" for Teacher 1.
  2. Teacher logs in and views Classes page.
- Expected:
  - "History 201" appears for Teacher 1.

#### AT-CROSS-02: Announcements visible to Teachers
- Steps:
  1. Admin posts "Welcome to the new term."
  2. Teacher logs in and views landing page.
- Expected:
  - Announcement visible.

### Error Handling and UX
#### AT-UX-01: Friendly errors for missing data
- Steps:
  1. Attempt to submit incomplete forms in Admin and Teacher pages.
- Expected:
  - Inline validation messages.
  - No server crash or unhandled error page.

#### AT-UX-02: Not found or invalid page
- Steps:
  1. Navigate to a non-existent page.
- Expected:
  - Error page shown.

## 10) Traceability Matrix (Feature -> Tests)
- Authentication: AT-AUTH-01..05
- Admin teacher management: AT-ADM-01..02
- Admin class management: AT-ADM-03..04
- Admin student enrollment: AT-ADM-05..06
- Announcements: AT-ADM-07..08, AT-CROSS-02
- Teacher classes: AT-TEACH-01..02
- Attendance: AT-TEACH-03..05
- Grading: AT-TEACH-06..08
- Calendar events: AT-TEACH-09..11
- UX and errors: AT-UX-01..02

## 11) Defect Severity Guidelines
- High: prevents core workflow completion (login, attendance, grading).
- Medium: workflow completion with workaround.
- Low: cosmetic or minor UI issue.

## 12) Test Execution Notes
- Log results per case: Pass/Fail with notes.
- Capture screenshots for failures.
- Re-test failed cases after fixes.
