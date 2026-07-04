# Changelog

All notable changes to the LearnNova project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- Real-time chat system foundations (Database models created: `ChatMessage`).
- Advanced course analytics dashboards.

---

## [Phase 6] - Assessments (Assignments & Quizzes)

### Added
- Interactive `QuizEngineController` allowing Students to take quizzes.
- Teacher interfaces for creating and grading Quizzes and Assignments.
- `AssignmentSubmission` logic with file tracking and grading capabilities.
- `QuizAttempt` tracking to record student scores and completion times.

---

## [Phase 5] - Student Enrollment & Learning

### Added
- `LearningController` for an immersive student viewing experience of course content.
- Content progress tracking (`ContentProgress`) to visually show students what they have completed.
- Course catalog browsing capabilities.
- Seamless student enrollment workflow.

---

## [Phase 4] - Course Management

### Added
- `CourseController` and `CourseContentController` for full CRUD operations by Teachers.
- Ability for Teachers to structure courses into modules and lessons.
- Capability to attach media, text, and external resources to course contents.

---

## [Phase 3] - User Dashboards

### Added
- Dedicated `AdminController`, `TeacherController`, and routing specifically designed for role-based views.
- High-level overview dashboards summarizing platform metrics for Admins and Teachers.
- `StudentAssignmentController` to centralize student tasks.

---

## [Phase 2] - Core Entities & Repositories

### Added
- Core Domain Models: `Course`, `Enrollment`, `CourseContent`.
- Data Access Layer abstraction through the implementation of the Repository Pattern.
- Dependency Injection configuration for Repositories in `Extensions/`.

---

## [Phase 1] - Foundation & Identity

### Added
- Initial ASP.NET Core MVC project structure.
- Implementation of `ApplicationDbContext`.
- ASP.NET Core Identity integration configured with custom `ApplicationUser`.
- Database seeding logic (`DbInitializer.cs`) to auto-generate standard roles (`Admin`, `Teacher`, `Student`) and a default Admin account.
- Basic Authentication workflows (Login, Logout, Access Denied).
