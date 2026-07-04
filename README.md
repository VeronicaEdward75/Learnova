# LearnNova

![LearnNova Banner](https://via.placeholder.com/1200x300?text=LearnNova+-+Next+Generation+Learning+Management+System)

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](#)

LearnNova is a comprehensive, modern Learning Management System (LMS) designed to bridge the gap between educators and students. Built on the robust ASP.NET Core MVC framework, LearnNova provides a secure, scalable, and intuitive platform for course management, interactive learning, and academic progress tracking.

---

## 📑 Table of Contents

- [About](#about)
- [Features](#features)
  - [Admin](#admin)
  - [Teacher](#teacher)
  - [Student](#student)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Database](#database)
- [Installation](#installation)
- [Database Setup](#database-setup)
- [Seed Data](#seed-data)
- [Configuration](#configuration)
- [Running the Project](#running-the-project)
- [Authentication](#authentication)
- [Screenshots](#screenshots)
- [UI / UX](#ui--ux)
- [Security](#security)
- [Future Improvements](#future-improvements)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)
- [Authors](#authors)
- [Version History](#version-history)

---

## 📖 About

LearnNova aims to solve the problem of fragmented online education tools by bringing together content delivery, assessments, and communication into a single, unified ecosystem.

**Target Users:**
- **Students**: Learners seeking an organized, engaging, and trackable educational experience.
- **Teachers**: Educators who need powerful tools to create courses, manage content, and evaluate student performance.
- **Admins**: System administrators responsible for user governance, system oversight, and platform health.

---

## ✨ Features

### 🛡️ Admin
- **User Management**: Complete control over user accounts.
- **Teacher Approval**: Review and approve new teacher registrations.
- **Student Approval**: Manage student enrollments and statuses.
- **Course Monitoring**: Oversee all courses created on the platform.
- **Dashboard**: High-level metrics and system oversight.
- **Security**: Manage roles and ensure platform integrity.

### 🎓 Teacher
- **Dashboard**: Quick overview of active courses and student engagement.
- **Course Management**: Create, edit, and publish courses.
- **Course Content**: Organize curriculum into modules and lessons.
- **Upload Materials**: Share resources (documents, videos, etc.) with students.
- **Assignments**: Create assignments and grade student submissions.
- **Quiz Management**: Build and manage quizzes with multiple question types.
- **Student Analytics**: Track student progress and performance within their courses.

### 📚 Student
- **Browse Courses**: Explore available courses in the catalog.
- **Enrollment**: Seamlessly enroll in active courses.
- **Learning Player**: An immersive interface for consuming course content.
- **Progress Tracking**: Visual indicators of completed modules and lessons.
- **Assignments**: Submit homework and view grades.
- **Quiz Engine**: Take interactive quizzes directly within the platform.
- **Quiz History**: Review past quiz attempts and scores.

---

## 🛠️ Technologies

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Backend** | ASP.NET Core MVC | Web framework for routing and controllers |
| **ORM** | Entity Framework Core | Database access and migrations |
| **Database** | SQL Server | Relational database management |
| **Security** | ASP.NET Core Identity | Authentication and role-based authorization |
| **Architecture**| Repository Pattern | Data access abstraction |
| **IoC** | Dependency Injection | Built-in .NET DI container |
| **Frontend** | HTML5 / CSS3 / JS | Core web technologies |
| **Styling** | Bootstrap | Responsive UI framework |

---

## 🏛️ Architecture

For detailed architectural information, please see [ARCHITECTURE.md](ARCHITECTURE.md).

The project follows a robust **N-Tier Architecture**:
- **Presentation Layer (Web)**: ASP.NET Core MVC (Thin Controllers, Views, ViewModels).
- **Service Layer**: Business logic encapsulation (e.g., `CourseService`, `QuizService`).
- **Data Access Layer**: Repository Pattern interacting with Entity Framework Core.
- **Domain Layer**: Entity Models (e.g., `Course`, `Enrollment`, `ApplicationUser`).

---

## 📁 Project Structure

For a full breakdown of the directory structure, please see [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md).

```text
LearnNova/
├── Controllers/       # HTTP request handlers (Thin controllers)
├── Models/            # Domain entities, ViewModels, and Enums
├── Services/          # Business logic and cross-cutting concerns
├── Repositories/      # Data access abstraction layer
├── Data/              # EF Core DbContext, Migrations, and Seed logic
├── Views/             # Razor views organized by controller
├── wwwroot/           # Static assets (CSS, JS, Images)
├── Extensions/        # Dependency Injection and utility extensions
├── appsettings.json   # Application configuration
└── Program.cs         # Application entry point and middleware pipeline
```

---

## 🗄️ Database

For an in-depth look at the database schema, please see [DATABASE.md](DATABASE.md).

The application uses **SQL Server** with **Entity Framework Core (Code-First)**.
Main entities include:
- `ApplicationUser` (Identity)
- `Course` & `CourseContent`
- `Enrollment` & `ContentProgress`
- `Assignment` & `AssignmentSubmission`
- `Quiz`, `QuizQuestion`, & `QuizAttempt`
- `StudentQuestion` & `ChatMessage`

---

## 🚀 Installation

For detailed setup instructions, please refer to [SETUP.md](SETUP.md).

### Quick Start
1. **Clone project**
   ```bash
   git clone <repository-url>
   cd LearnNova/LearnNova
   ```
2. **Restore packages**
   ```bash
   dotnet restore
   ```
3. **Build**
   ```bash
   dotnet build
   ```

---

## ⚙️ Database Setup

1. Ensure SQL Server is running and accessible.
2. Update the `ConnectionStrings:DefaultConnection` in `appsettings.json`.
3. Run Entity Framework migrations to create the database:
   ```bash
   dotnet ef database update
   ```

---

## 🌱 Seed Data

The project automatically seeds the following on startup if they do not exist:
- **Roles**: `Admin`, `Teacher`, `Student`
- **Admin Account**: 
  - **Email**: `admin@learnnova.com`
  - **Password**: `Admin@12345`

*Note: For security, change the default admin password immediately after deployment.*

---

## 🔧 Configuration

All configurations are managed in `appsettings.json` and environment variables. Key settings include:
- **Connection Strings**: Target database configuration.
- **Identity Settings**: Password complexity and lockout policies.
- **Environment**: Controls developer exception pages and asset bundling.

---

## 🏃 Running the Project

1. Open a terminal in the `LearnNova` directory.
2. Execute the run command:
   ```bash
   dotnet run
   ```
3. Open your browser and navigate to `https://localhost:5001` (or the port specified in your console output).

---

## 🔐 Authentication

The platform utilizes ASP.NET Core Identity with Role-Based Access Control (RBAC).

- **Admin**: Full system access, user management, and global oversight.
- **Teacher**: Restricted to managing their own courses, materials, quizzes, and student grading.
- **Student**: Restricted to viewing enrolled courses, submitting assignments, and taking quizzes.

---

## 🖼️ Screenshots

*Placeholders for actual application screenshots.*

- **Landing Page**: `![Landing Page](docs/screenshots/landing.png)`
- **Admin Dashboard**: `![Admin Dashboard](docs/screenshots/admin-dash.png)`
- **Teacher Dashboard**: `![Teacher Dashboard](docs/screenshots/teacher-dash.png)`
- **Student Dashboard**: `![Student Dashboard](docs/screenshots/student-dash.png)`
- **Course Details**: `![Course Details](docs/screenshots/course-details.png)`
- **Learning Player**: `![Learning Player](docs/screenshots/learning-player.png)`
- **Assignments**: `![Assignments](docs/screenshots/assignments.png)`
- **Quiz Engine**: `![Quiz Engine](docs/screenshots/quiz-engine.png)`
- **Analytics**: `![Analytics](docs/screenshots/analytics.png)`

---

## 🎨 UI / UX

- **Design System**: Built on Bootstrap with custom CSS overrides for a modern, flat-design aesthetic.
- **Responsive Design**: Fully mobile-responsive, ensuring students can learn on any device.
- **Animations**: Subtle CSS transitions for interactive elements (hover states, modal reveals).
- **Accessibility**: Semantic HTML and ARIA labels implemented across key components.

---

## 🛡️ Security

- **Identity**: Secure password hashing and account management via ASP.NET Core Identity.
- **Authorization**: Strict `[Authorize(Roles = "...")]` attributes on controllers and actions.
- **Validation**: Comprehensive server-side and client-side model validation.
- **Anti-Forgery**: Cross-Site Request Forgery (CSRF) protection tokens on all forms.
- **Exception Handling**: Global exception handling to prevent leaking stack traces in production.
- **Data Protection**: Entity Framework prevents SQL Injection through parameterized queries.

---

## 🔮 Future Improvements

- Implementation of a real-time Chat System using SignalR.
- Integration with external payment gateways (Stripe/PayPal) for paid courses.
- Advanced reporting and analytics export (PDF/Excel) for Teachers and Admins.
- Gamification elements (Badges, Leaderboards) for Students.
- Video streaming optimizations and integration with cloud storage (AWS S3/Azure Blob).

---

## 🐛 Troubleshooting

- **Migration Issues**: If `dotnet ef database update` fails, ensure the connection string is correct and the SQL Server service is running. Try dropping the database and re-running if local schema gets corrupted.
- **Port Already in Use**: Change the applicationUrl in `Properties/launchSettings.json` or use `dotnet run --urls="https://localhost:5005"`.
- **Package Restore Errors**: Ensure you have the .NET SDK installed (matching the `global.json` or project version) and internet connectivity to nuget.org.

---

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:
- **Coding Standards**: Follow standard C# naming conventions and clean code principles.
- **Repository Pattern**: Ensure all database access goes through Repositories, not directly in Controllers.
- **Commit Style**: Use descriptive, imperative commit messages (e.g., `Add quiz engine functionality`, not `fixed stuff`).

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Authors

- **[Your Name/Team Name]** - *Initial work* - [GitHub Profile](#)
- **[Contributor Name]** - *Frontend Development* - [GitHub Profile](#)
- **[Contributor Name]** - *Backend & Database* - [GitHub Profile](#)

---

## 📜 Version History

- **Phase 1: Foundation & Identity**
  - Project setup, DbContext, ASP.NET Core Identity integration.
- **Phase 2: Core Entities & Repositories**
  - Domain models (Course, Enrollment) and Repository abstractions.
- **Phase 3: User Dashboards**
  - Admin, Teacher, and Student specific routing and views.
- **Phase 4: Course Management**
  - CRUD operations for Courses and Course Contents by Teachers.
- **Phase 5: Student Enrollment & Learning**
  - Course catalog browsing, enrollment logic, and Content Progress tracking.
- **Phase 6: Assessments (Assignments & Quizzes)**
  - Assignment submission system and interactive Quiz Engine implementation.
