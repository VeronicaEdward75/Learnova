# Database Documentation

The LearnNova platform utilizes a relational database managed by **SQL Server** and accessed via **Entity Framework Core (EF Core)** using the Code-First approach.

---

## Technology Stack

- **RDBMS**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **Schema Management**: EF Core Migrations
- **Authentication/Identity**: ASP.NET Core Identity (Customized)

---

## Core Schema and Relationships

The database is built around several key entities that represent the educational domain.

### 1. ApplicationUser (Identity)
Inherits from `IdentityUser`. Represents all users in the system.
- **Discriminators / Roles**: Admin, Teacher, Student (managed via `UserRole` enum and Identity Roles).
- **Relationships**:
  - `Teacher` -> 1:N -> `Courses` (CoursesTaught)
  - `Student` -> 1:N -> `Enrollments`
  - `Student` -> 1:N -> `QuizAttempts`
  - `Student` -> 1:N -> `AssignmentSubmissions`
  - `User` -> 1:N -> `SentMessages` & `ReceivedMessages`

### 2. Course Management
- **Course**: The primary educational container.
  - Belongs to one `Teacher` (ApplicationUser).
  - Contains many `CourseContent` items (Modules/Lessons).
  - Contains many `Assignments` and `Quizzes`.
- **CourseContent**: The actual learning materials (video links, text, documents).
  - Belongs to one `Course`.

### 3. Student Enrollment & Progress
- **Enrollment**: A join table mapping a `Student` to a `Course`.
  - Tracks enrollment date and status.
- **ContentProgress**: Tracks whether a specific `Student` has completed a specific `CourseContent`.

### 4. Assessments (Quizzes)
- **Quiz**: A collection of questions attached to a `Course`.
- **QuizQuestion**: Individual questions belonging to a `Quiz`. Can be multiple choice, true/false, etc.
- **QuizAttempt**: Records a `Student` taking a `Quiz`, including their score and submission time.

### 5. Assessments (Assignments)
- **Assignment**: A task required by a Teacher for a `Course`, usually requiring a file upload or text submission.
- **AssignmentSubmission**: A `Student`'s response to an `Assignment`, including file paths, submission dates, and grades awarded by the Teacher.

### 6. Communication
- **StudentQuestion**: A Q&A feature allowing students to ask questions related to a `Course`.
- **ChatMessage**: Enables direct messaging between users (e.g., Student to Teacher).

---

## Entity Framework Configurations

The `ApplicationDbContext.cs` utilizes the Fluent API (`OnModelCreating`) to strictly define relationships and cascading delete behaviors to prevent SQL Server cyclical cascade path errors.

### Cascade Delete Rules

To maintain data integrity and prevent cyclic dependencies:
- Deleting a `Course` **cascades** down to delete its `Enrollments`, `CourseContents`, `Quizzes`, and `Assignments`.
- `ApplicationUser` relationships (Student/Teacher IDs on various entities) are set to `DeleteBehavior.Restrict`. 
  - *Best Practice*: Users should be deactivated (soft-deleted via an `IsActive` flag) rather than hard-deleted from the database to preserve historical educational records (grades, submissions, chat logs).

---

## Migrations Workflow

Schema changes are managed exclusively through EF Core Migrations.

**To add a new migration after modifying an Entity model:**
```bash
dotnet ef migrations add <DescriptiveMigrationName>
```

**To apply pending migrations to the database:**
```bash
dotnet ef database update
```

**To revert the database to a previous state:**
```bash
dotnet ef database update <PreviousMigrationName>
```
