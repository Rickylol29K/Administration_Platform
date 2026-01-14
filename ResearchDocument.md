# Research Document – Administration Platform

Written by: [Your name]

Date: [DD/MM/YYYY]

Version: 1.0

## 1. Introduction

### 1.1 Context
This project is a small administration platform for a school-like setting. The system supports two roles: Admin and Teacher. Admins create teacher accounts, create classes, add students to classes, and manage announcements. Teachers log in to view their classes, take attendance, enter grades, and manage calendar events. The focus is on a simple, server-rendered web app with a clear structure and a relational database.

### 1.2 Research Question
How can we build a simple and maintainable administration platform that supports class management, attendance, grading, events, and announcements for Admins and Teachers?

### 1.3 Sub-Questions
1. What are the core features and user roles required for the platform?
2. Why use Razor Pages instead of MVC for this project?
3. Why use MSSQL as the database for this project?
4. How should the data access and business logic be structured to stay readable and testable?
5. How do we validate the solution works as expected?

## 2. Research Approach
We used a lightweight approach based on:
- Requirements analysis (reviewing needed user actions and data).
- Architecture comparison (Razor Pages vs MVC, MSSQL vs alternatives).
- Prototype validation (building and reviewing the working app flows).
- Test review (checking the existing automated tests and expected outcomes).

## 3. Research Outcomes

### 3.1 Sub-question 1: What are the core features and user roles required for the platform?
**Method:** Requirements analysis based on the existing project scope.

**Findings:**
- Roles: Admin and Teacher.
- Admin responsibilities: create teacher accounts, create classes, add students, publish announcements.
- Teacher responsibilities: view classes, load class roster, record attendance by date, create grade sheets, manage calendar events.
- Shared: login, role-based access, and session-based authentication.

**Conclusion:** The system needs straightforward role-based pages with predictable CRUD operations and simple validation rules.

### 3.2 Sub-question 2: Why use Razor Pages instead of MVC?
**Method:** Architecture comparison and fit to scope.

**Findings:**
- Razor Pages emphasizes a page-focused model, which matches this project’s page-centric flows (Login, Admin Index, Teacher pages).
- It reduces controller routing and keeps handlers close to the page they serve, which improves readability for a small team.
- Less boilerplate is required than full MVC (no separate controller per view), which helps keep the project simple.
- MVC can be more flexible for complex API-heavy apps, but this project is mostly server-rendered forms and tables.

**Conclusion:** Razor Pages is the most boring and readable choice for a small, form-driven app. It aligns with the project scope and keeps code close to the UI.

### 3.3 Sub-question 3: Why use MSSQL as the database?
**Method:** Technology comparison based on data needs.

**Findings:**
- The data is relational: Users, Classes, Students, Enrollments, Attendance, Grades, Events, and Announcements.
- MSSQL provides strong relational integrity, indexing, and transactional safety for attendance and grading updates.
- MSSQL integrates well with .NET and common deployment setups for ASP.NET projects.
- SQLite is lighter but not ideal for multi-user concurrency; PostgreSQL is a good alternative, but MSSQL is a common default for .NET stacks and course tooling.

**Conclusion:** MSSQL is a reliable, conventional choice for a relational, multi-user school administration app and reduces risk during development.

### 3.4 Sub-question 4: How should data access and business logic be structured to stay readable and testable?
**Method:** Architecture design using clear separation of concerns.

**Findings:**
- The project uses a Data Access Layer (DAL) for SQL operations and a Logic layer for business rules.
- A repository interface (`IDataRepository`) allows the Logic layer to be tested with fake data sources.
- This separation keeps SQL concerns out of the page models and keeps the pages focused on HTTP input/output.

**Conclusion:** A simple layered architecture (Pages → Logic → DAL) keeps the code boring, easy to follow, and testable.

### 3.5 Sub-question 5: How do we validate the solution works as expected?
**Method:** Review of existing automated tests.

**Findings:**
- Tests cover authentication, class logic, attendance logic, grading logic, event logic, and announcements.
- Page-level tests validate that the correct behavior occurs for common user flows.

**Conclusion:** The existing tests provide a baseline for correctness and reduce the risk of regressions when refactoring or extending the app.

## 4. Main Question Conclusion
The project can be implemented cleanly by using Razor Pages for a page-focused UI, MSSQL for a robust relational database, and a clear separation between pages, logic, and data access. This structure is simple, readable, and well-suited for the small, form-driven scope of the administration platform.

## 5. Sources
Microsoft. (n.d.). *ASP.NET Core Razor Pages*. Microsoft Learn. https://learn.microsoft.com/aspnet/core/razor-pages/

Microsoft. (n.d.). *ASP.NET Core MVC*. Microsoft Learn. https://learn.microsoft.com/aspnet/core/mvc/

Microsoft. (n.d.). *SQL Server documentation*. Microsoft Learn. https://learn.microsoft.com/sql/sql-server/

Peters, R. (2025). *Administration Platform codebase and automated tests* (Version 1.0) [Computer software]. Local project repository.
