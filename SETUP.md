# Setup Guide

This document provides detailed instructions on how to install, configure, and run the LearnNova platform locally.

## Prerequisites

Ensure you have the following installed on your development machine:
- [.NET SDK 8.0/9.0](https://dotnet.microsoft.com/download) (Check your `global.json` or `.csproj` for exact version)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer Edition)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) OR [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit
- [Git](https://git-scm.com/)

---

## 1. Installation

### Clone the Repository

Open your terminal or command prompt and run:

```bash
git clone <repository-url>
cd Learnova-initialEF/LearnNova
```

### Restore Packages

Restore the NuGet dependencies required for the project:

```bash
dotnet restore
```

---

## 2. Configuration

### Connection String

1. Open `appsettings.json` (or `appsettings.Development.json` for local dev) located in the `LearnNova` project directory.
2. Locate the `ConnectionStrings` section.
3. Update the `DefaultConnection` to match your local SQL Server instance.

**Example for LocalDB:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LearnNovaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

**Example for SQL Express:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=LearnNovaDb;Trusted_Connection=True;Encrypt=False"
}
```

---

## 3. Database Setup

The project uses Entity Framework Core Code-First Migrations.

### Applying Migrations

To create the database and apply the schema:

1. Ensure your terminal is in the `LearnNova` directory.
2. Run the following command:

```bash
dotnet ef database update
```

*Note: If you do not have the EF Core tools installed, you can install them globally via: `dotnet tool install --global dotnet-ef`.*

### Restoring a Database Backup (.bak)

If you were provided with a SQL Server backup file (`.bak`) instead of using migrations:

1. Open **SQL Server Management Studio (SSMS)**.
2. Connect to your local SQL Server instance.
3. Right-click on **Databases** and select **Restore Database...**
4. Choose **Device** and browse to select the `.bak` file.
5. Click **OK** to restore.
6. Ensure your `appsettings.json` connection string points to this restored database.

---

## 4. Seed Data

The application includes an automatic seeding mechanism (`DbInitializer.cs`) that runs when the application starts.

It automatically provisions:
- **Roles**: `Admin`, `Teacher`, `Student`
- **Default Admin Account**:
  - **Email**: `admin@learnnova.com`
  - **Password**: `Admin@12345`

You can use these credentials to log in for the first time and begin setting up other users (Teachers and Students).

---

## 5. Running the Project

### Using .NET CLI

1. Open your terminal in the `LearnNova` folder.
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the project:
   ```bash
   dotnet run
   ```
4. The console output will provide the local URL (e.g., `https://localhost:7123`). Open this URL in your web browser.

### Using Visual Studio

1. Double-click the `LearnNova.slnx` or open the folder in Visual Studio.
2. Ensure `LearnNova` is set as the Startup Project.
3. Press **F5** (Run with Debugging) or **Ctrl+F5** (Run without Debugging).
4. The browser will launch automatically.

---

## 6. Troubleshooting

- **Database Update Fails**: Verify your SQL Server is running and the connection string exactly matches your instance name.
- **Port Conflicts**: If you receive a "port already in use" error, you can modify the ports in `Properties/launchSettings.json`.
- **Missing Migrations**: If `dotnet ef database update` says it doesn't recognize the command, ensure EF Core tools are installed and your terminal is inside the project directory containing the `.csproj` file.
