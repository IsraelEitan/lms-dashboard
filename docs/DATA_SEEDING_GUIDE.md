# Data Seeding Guide

This guide explains how to use the data seeding feature in the LMS Dashboard application.

---

## Overview

The LMS Dashboard includes an automated data seeding system that populates the application with sample data for development and testing purposes. This feature is **configurable** and **idempotent**, meaning it's safe to run multiple times without creating duplicates.

---

## Quick Start

### Automatic Seeding (Development)

By default, data seeding is **enabled in development** and runs automatically when you start the application:

```bash
cd src/Lms.Api
dotnet run
```

The application will:
1. Check if data already exists
2. If empty, seed 15 students, 12 courses, and 13 enrollments
3. Log the seeding process to console and log files

### Using the Startup Script

The easiest way to start with seeded data:

```powershell
# From project root
.\start-app.ps1
```

This script:
- Builds and starts the backend (with seeding enabled)
- Starts the frontend
- Opens the browser automatically

---

## Configuration

### Enable/Disable Seeding

Seeding is controlled via `appsettings.json`:

**Development (Enabled):**
```json
// appsettings.Development.json
{
  "DataSeeding": {
    "Enabled": true
  }
}
```

**Production (Disabled):**
```json
// appsettings.json
{
  "DataSeeding": {
    "Enabled": false
  }
}
```

### Override Configuration

You can override seeding via environment variables:

```bash
# Disable seeding in development
export DataSeeding__Enabled=false
dotnet run

# Enable seeding in production (not recommended)
export DataSeeding__Enabled=true
dotnet run
```

---

## What Gets Seeded

### Students (15)

Sample students with realistic names and university email addresses:

| ID | Name | Email |
|----|------|-------|
| Auto-generated GUID | Shai Zuckerberg | shai.zuckerberg@university.il |
| Auto-generated GUID | Dana Muskowitz | dana.muskowitz@university.il |
| ... | ... | ... |

**All 15 students:**
1. Shai Zuckerberg
2. Dana Muskowitz
3. Eitan Altman
4. Noa Ben-Gates
5. Omer Nadella
6. Liron Sandberg
7. Tamar Jobsman
8. Yair Brinstein
9. Gal Pichai
10. Roni Bezosov
11. Tal Friedmanberg
12. Itay Cookman
13. Maya Chesky
14. Oren Nadavsky
15. Aviv Kalanik

### Courses (12)

A diverse curriculum spanning multiple subjects:

| Code | Title | Description |
|------|-------|-------------|
| CS101 | Introduction to Computer Science | Fundamentals of programming |
| CS201 | Data Structures and Algorithms | Core data structures |
| CS301 | Database Systems | Relational databases, SQL |
| CS401 | Software Engineering | Development methodologies |
| MATH101 | Calculus I | Limits, derivatives, integrals |
| MATH201 | Linear Algebra | Vector spaces, matrices |
| PHYS101 | Physics I: Mechanics | Classical mechanics |
| ENG101 | English Composition | Academic writing |
| BIO101 | Introduction to Biology | Cell biology, genetics |
| CHEM101 | General Chemistry | Atomic structure, bonding |
| HIST201 | World History | Major civilizations |
| PSY101 | Introduction to Psychology | Human behavior |

### Enrollments (13)

Sample enrollments connecting students to courses:

| Student | Course | Description |
|---------|--------|-------------|
| Shai Zuckerberg | CS101 | Multiple students in CS101 |
| Shai Zuckerberg | MATH101 | Same student, multiple courses |
| Dana Muskowitz | CS101 | |
| Dana Muskowitz | CS201 | |
| ... | ... | 13 total enrollments |

This creates a realistic distribution where:
- Some courses have multiple students (CS101 has 3 students)
- Some students take multiple courses (Shai and Dana)
- Some courses have no enrollments (for testing empty states)

---

## How It Works

### Seeding Process

The seeding happens in `Program.cs` during application startup:

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
  var seedingEnabled = app.Configuration.GetValue<bool>("DataSeeding:Enabled");
  if (seedingEnabled)
  {
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
  }
}
```

### Idempotency (Safe to Run Multiple Times)

The `DataSeeder` checks if data exists before seeding:

```csharp
public async Task SeedAsync(CancellationToken ct = default)
{
  // Check if data already exists
  var existingStudents = await _students.GetAllAsync(ct);
  var existingCourses = await _courses.GetAllAsync(ct);
  var existingEnrollments = await _enrollments.QueryAsync(...);

  if (existingStudents.Value?.Count > 0 || 
      existingCourses.Value?.Count > 0 || 
      existingEnrollments.Value?.Items.Count > 0)
  {
    _logger.LogInformation("Data already exists. Skipping seeding");
    return;  // Exit early, don't seed
  }

  // Only seed if database is empty
  await SeedStudentsAsync(ct);
  await SeedCoursesAsync(ct);
  await SeedEnrollmentsAsync(studentIds, courseIds, ct);
}
```

**Key Features:**
- Checks all three entities (students, courses, enrollments)
- If ANY data exists, skips seeding entirely
- Safe to run multiple times without duplicates
- Logs all actions for debugging

### Logging

Seeding operations are fully logged:

```
[2025-11-02 14:30:15 INF] Starting data seeding process
[2025-11-02 14:30:16 INF] Seeded 15 students successfully
[2025-11-02 14:30:16 INF] Seeded 12 courses successfully
[2025-11-02 14:30:16 INF] Seeded 13 enrollments successfully
[2025-11-02 14:30:16 INF] Data seeding completed successfully
```

Or if data exists:
```
[2025-11-02 14:30:15 INF] Starting data seeding process
[2025-11-02 14:30:15 INF] Data already exists. Skipping seeding
```

---

## Manual Seeding via API

While automatic seeding is recommended, you can manually manage data via the API endpoints.

### Clear All Data (Testing Only)

**Warning:** There's no built-in "clear database" endpoint. Since we use in-memory storage, simply restart the application to reset data.

```bash
# Restart the application
dotnet run
```

### Manually Create Data via API

You can also use Swagger UI to manually create test data:

1. Navigate to http://localhost:5225/swagger
2. Use POST endpoints to create:
   - `/api/v1/students` - Create students
   - `/api/v1/courses` - Create courses
   - `/api/v1/enrollments` - Create enrollments

---

## Customizing Seed Data

To modify the seeded data, edit `src/Lms.Api/Infrastructure/Data/DataSeeder.cs`:

### Adding More Students

```csharp
private async Task<List<Guid>> SeedStudentsAsync(CancellationToken ct)
{
  var students = new List<CreateStudentDto>
  {
    new() { Name = "Your Name", Email = "your.email@university.edu" },
    // Add more students here
  };
  
  // Rest of the method...
}
```

### Adding More Courses

```csharp
private async Task<List<Guid>> SeedCoursesAsync(CancellationToken ct)
{
  var courses = new List<CreateUpdateCourseDto>
  {
    new() { 
      Code = "CS501", 
      Title = "Machine Learning", 
      Description = "Introduction to ML" 
    },
    // Add more courses here
  };
  
  // Rest of the method...
}
```

### Changing Enrollments

```csharp
private async Task SeedEnrollmentsAsync(List<Guid> studentIds, List<Guid> courseIds, CancellationToken ct)
{
  var enrollments = new List<(Guid StudentId, Guid CourseId)>
  {
    (studentIds[0], courseIds[0]),  // First student, first course
    // Add more enrollments here
  };
  
  // Rest of the method...
}
```

---

## Troubleshooting

### Seeding Doesn't Run

**Problem:** Application starts but no data appears.

**Solutions:**
1. Check configuration:
   ```json
   "DataSeeding": { "Enabled": true }
   ```
2. Check logs for seeding messages
3. Verify you're in Development environment:
   ```bash
   echo $ASPNETCORE_ENVIRONMENT  # Should be "Development"
   ```

### Duplicate Data

**Problem:** Running the application multiple times creates duplicates.

**Solution:** This shouldn't happen due to idempotency checks. If it does:
1. Check the `SeedAsync` method logic
2. Verify the existence checks are working
3. Restart the application (in-memory data will be cleared)

### Seeding Fails with Errors

**Problem:** Seeding throws exceptions.

**Solutions:**
1. Check logs for specific error messages
2. Verify validation rules (e.g., email format)
3. Check for service registration in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<DataSeeder>();
   ```

### Can't Disable Seeding

**Problem:** Seeding runs even when disabled.

**Solutions:**
1. Verify `appsettings.Development.json` has:
   ```json
   "DataSeeding": { "Enabled": false }
   ```
2. Check environment-specific config is being loaded
3. Clear any environment variable overrides

---

## Best Practices

### Development

- **Always enable seeding** in development for consistent test data
- **Use the provided seed data** to test all features
- **Don't modify** the seeded data structure without updating tests

### Testing

- **Reset data** by restarting the application
- **Test with empty state** by disabling seeding temporarily
- **Test with seeded data** to verify UI displays correctly

### Production

- **Always disable seeding** in production
- **Never** use seed data in production environments
- **Verify** seeding is disabled before deploying:
  ```bash
  grep -r "DataSeeding" appsettings.json
  # Should show "Enabled": false
  ```

---

## Production Migration

When moving to a production database (SQL Server, PostgreSQL, etc.):

### Option 1: Remove Seeding

If you don't need seeding in production:

```csharp
// Program.cs - Remove the seeding block entirely
// OR keep it but ensure appsettings.json has Enabled: false
```

### Option 2: Use EF Core Migrations

For production-ready seeding with Entity Framework:

```csharp
public class ApplicationDbContextSeed
{
  public static async Task SeedAsync(ApplicationDbContext context)
  {
    if (!await context.Students.AnyAsync())
    {
      // Seed initial data
      context.Students.AddRange(/* seed data */);
      await context.SaveChangesAsync();
    }
  }
}

// Call in Program.cs
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await ApplicationDbContextSeed.SeedAsync(context);
```

---

## Summary

**Key Points:**
- Seeding is **automatic** in development
- Seeding is **idempotent** (safe to run multiple times)
- Configuration via `appsettings.json` or environment variables
- Creates 15 students, 12 courses, 13 enrollments
- Fully logged for debugging
- Easy to customize by editing `DataSeeder.cs`

**Quick Commands:**
```bash
# Start with seeding (default in dev)
dotnet run

# Start without seeding
export DataSeeding__Enabled=false
dotnet run

# Reset all data
# Just restart the application (in-memory storage)
```

---

For more information, see:
- [Backend README](../src/Lms.Api/README.md)
- [Testing Guide](TESTING_GUIDE.md)
- Source: `src/Lms.Api/Infrastructure/Data/DataSeeder.cs`

