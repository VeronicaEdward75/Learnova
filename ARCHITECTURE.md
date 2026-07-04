# Architecture Overview

LearnNova is architected to be maintainable, scalable, and testable, adhering to industry best practices for ASP.NET Core applications.

---

## Architectural Pattern: N-Tier Architecture

The application is structured into logical layers to separate concerns, making the codebase easier to manage and extend.

### 1. Presentation Layer (Web)
- **Framework**: ASP.NET Core MVC.
- **Thin Controllers**: Controllers are intentionally kept "thin". Their primary responsibility is to handle HTTP requests, validate `ModelState`, delegate business logic to the Service Layer, and return the appropriate View or redirection.
- **Views**: Razor Views (`.cshtml`) render the UI based on strongly-typed ViewModels.
- **ViewModels**: Data transfer objects specifically tailored for Views. They decouple the Presentation Layer from the Domain Entities, ensuring Views only receive the data they need.

### 2. Service Layer (Business Logic)
- **Role**: Contains all business rules, calculations, and complex workflows.
- **Implementation**: Interfaces and Classes (e.g., `ICourseService`, `CourseService`).
- **Benefits**: Decouples business logic from HTTP contexts. Services can be easily unit tested without mocking HttpContexts or Web objects.

### 3. Data Access Layer (Repository Pattern)
- **Role**: Abstracts the underlying database technology (Entity Framework Core).
- **Implementation**: Generic or specific Repositories (e.g., `IRepository<T>`, `ICourseRepository`).
- **Benefits**: Centralizes data access logic. If the ORM or database technology changes in the future, modifications are isolated to this layer.

### 4. Domain Layer (Entity Models)
- **Role**: Represents the core data structures and schema mapping of the application.
- **Implementation**: C# POCOs (Plain Old CLR Objects) annotated with Data Annotations or configured via Fluent API in the DbContext.

---

## Core Design Patterns

### Dependency Injection (DI)

LearnNova heavily utilizes the built-in ASP.NET Core Dependency Injection container.

- **Inversion of Control (IoC)**: High-level modules (Controllers) do not depend on low-level modules (Repositories or DbContext). Both depend on abstractions (Interfaces).
- **Registration**: Services and Repositories are registered in `Program.cs` (or via Extension methods like `AddApplicationServices()`) using appropriate lifecycles (`AddScoped`, `AddTransient`, `AddSingleton`).
  - *Note: Repositories and Services are typically registered as Scoped so they share the same DbContext instance per HTTP request.*

### Repository Pattern

The application abstracts `DbContext` calls behind Repository interfaces. 

```csharp
public interface ICourseRepository 
{
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course> GetByIdAsync(int id);
    Task AddAsync(Course course);
    // ...
}
```

This prevents EF Core-specific code (like `.Include()` or `.AsNoTracking()`) from bleeding into the Service or Presentation layers, ensuring a clean separation of concerns.

---

## File and Folder Structure Rationale

The project uses a standard MVC folder layout augmented with folders for Services and Repositories to support the N-Tier architecture:

- `Controllers/`: Entry points for user interaction. Organized by feature (e.g., `AdminController`, `CourseController`).
- `Services/`: Contains interfaces and implementations for business logic.
- `Repositories/`: Contains interfaces and implementations for data access.
- `Models/Entities/`: Database-mapped classes.
- `Models/ViewModels/`: DTOs used specifically for data passing between Controllers and Views.
- `Data/`: Contains the `ApplicationDbContext`, Migrations, and Seeding logic.

By enforcing this structure, developers can easily locate responsibilities within the codebase, leading to faster onboarding and more reliable feature development.
