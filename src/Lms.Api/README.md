# LMS Dashboard - Backend API

**ASP.NET Core 9.0 | Clean Architecture | RESTful API**

A production-ready Learning Management System API demonstrating enterprise-level architecture patterns, scalability considerations, and best practices.

---

## 📋 Table of Contents

- [Quick Start](#quick-start)
- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Architecture Decisions](#architecture-decisions)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Development](#development)
- [Testing](#testing)

---

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run the API

```bash
# From the project root
cd src/Lms.Api

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# API will be available at:
# - https://localhost:5225
# - http://localhost:5225
# - Swagger UI: https://localhost:5225/swagger
```

### Build & Test

```bash
# Build
dotnet build

# Run tests
dotnet test ../../tests/Lms.UnitTests/Lms.UnitTests.csproj

# Publish for production
dotnet publish -c Release -o ./publish
```

---

## 🏗️ Architecture Overview

This API follows **Clean Architecture** principles with clear layer separation and dependency rules.

### Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│              Presentation Layer                      │
│  ┌──────────────┐  ┌──────────────────────────┐    │
│  │ Controllers  │  │ Middleware & Extensions  │    │
│  └──────────────┘  └──────────────────────────┘    │
└───────────────────────┬─────────────────────────────┘
                        ↓ (depends on)
┌───────────────────────┴─────────────────────────────┐
│              Application Layer                       │
│  ┌───────────┐  ┌───────────┐  ┌────────────────┐  │
│  │ Services  │  │Interfaces │  │ DTOs/Contracts │  │
│  └───────────┘  └───────────┘  └────────────────┘  │
└───────────────────────┬─────────────────────────────┘
                        ↓ (depends on)
┌───────────────────────┴─────────────────────────────┐
│                Domain Layer                          │
│  ┌────────────┐  ┌──────────────────────────────┐  │
│  │   Models   │  │    Business Logic            │  │
│  └────────────┘  └──────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
                        ↑ (all depend on)
┌─────────────────────────────────────────────────────┐
│            Infrastructure Layer                      │
│  ┌──────────┐  ┌─────────┐  ┌──────────────────┐   │
│  │  Data    │  │ Caching │  │  Cross-Cutting   │   │
│  │(In-Mem)  │  │(Memory) │  │    Concerns      │   │
│  └──────────┘  └─────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────┘
```

### Dependency Rules
1. **Domain** has no dependencies (pure C#)
2. **Application** depends only on Domain
3. **Infrastructure** implements Application interfaces
4. **Presentation** coordinates and presents data

---

## 📁 Project Structure

```
Lms.Api/
│
├── Application/                  # Business Logic Layer
│   ├── Interfaces/              # Service contracts (public)
│   │   ├── ICourseService.cs
│   │   ├── IStudentService.cs
│   │   └── IEnrollmentService.cs
│   └── Services/                # Service implementations (internal sealed)
│       ├── CourseService.cs
│       ├── StudentService.cs
│       ├── EnrollmentService.cs
│       └── CacheKeys.cs
│
├── Contracts/                   # Data Transfer Objects
│   └── DTOs/
│       ├── CourseDtos.cs       # CourseDto, CreateUpdateCourseDto
│       ├── StudentDtos.cs      # StudentDto, CreateStudentDto, UpdateStudentDto
│       └── EnrollmentDtos.cs   # EnrollmentDto, AssignStudentRequest
│
├── Domain/                      # Domain Models (pure entities)
│   └── Models/
│       ├── Course.cs           # Course entity
│       ├── Student.cs          # Student entity
│       └── Enrollment.cs       # Enrollment entity
│
├── Infrastructure/              # Cross-Cutting Concerns
│   ├── Attributes/
│   │   └── IdempotencyAttribute.cs
│   ├── Data/
│   │   └── DataSeeder.cs       # Development data seeding
│   ├── Extensions/
│   │   └── ResultExtensions.cs # Result → ActionResult mapping
│   └── Middleware/
│       ├── ExceptionHandlingMiddleware.cs
│       └── IdempotencyMiddleware.cs
│
├── Presentation/                # API Presentation Layer
│   ├── Contracts/
│   │   └── Paging.cs           # PagingQuery, PagedResult<T>
│   └── Controllers/
│       ├── CoursesController.cs
│       ├── StudentsController.cs
│       └── EnrollmentsController.cs
│
├── Common/                      # Shared Utilities
│   ├── Constants.cs            # App-wide constants
│   └── Results/                # Result pattern implementation
│       ├── Results.cs
│       └── Error.cs
│
└── Program.cs                   # Application entry point
```

---

## 🎯 Architecture Decisions

### 1. **Clean Architecture**

**Decision:** Use Clean Architecture with distinct layers.

**Rationale:**
- ✅ **Testability**: Each layer can be tested independently
- ✅ **Maintainability**: Changes isolated to specific layers
- ✅ **Flexibility**: Easy to swap implementations (e.g., database)
- ✅ **Separation of Concerns**: Business logic separate from infrastructure

**Trade-offs:**
- More files and folders (but better organized)
- Initial learning curve (but pays off long-term)

---

### 2. **Result Pattern (Railway-Oriented Programming)**

**Decision:** Use `Result<T>` instead of throwing exceptions for business logic failures.

**Example:**
```csharp
// ✅ DO: Return Result for expected failures
public async Task<Result<StudentDto>> CreateAsync(CreateStudentDto dto, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return Result<StudentDto>.Failure(Errors.Common.Validation("Name required"));
    
    // ... create student
    return Result<StudentDto>.Success(studentDto);
}

// ❌ DON'T: Throw exceptions for validation
public async Task<StudentDto> CreateAsync(CreateStudentDto dto, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ValidationException("Name required");  // ❌ Expensive!
}
```

**Rationale:**
- ✅ **Performance**: Exceptions are expensive (stack unwinding)
- ✅ **Explicit**: Caller must handle success/failure explicitly
- ✅ **Type-Safe**: Compiler ensures error handling
- ✅ **Cleaner**: No try-catch blocks everywhere

**When to use exceptions:**
- ❌ Unexpected errors (OutOfMemoryException, etc.)
- ❌ Unrecoverable failures

---

### 3. **In-Memory Data Storage**

**Decision:** Use `ConcurrentDictionary` for data storage instead of a database.

**Rationale:**
- ✅ **Simplicity**: No database setup required for demo
- ✅ **Fast**: Microsecond latency
- ✅ **Focus**: Demonstrates architecture, not database operations
- ✅ **Portable**: Works anywhere .NET runs

**Production Alternative:**
```csharp
// For production, replace with:
- Entity Framework Core + SQL Server/PostgreSQL
- Dapper + stored procedures
- MongoDB for document storage
- Redis for distributed caching
```

**Trade-offs:**
- Data lost on restart (acceptable for demo)
- Not scalable across multiple instances (add Redis for distributed)

---

### 4. **Two-Level Caching Strategy**

**Decision:** Implement caching at service and HTTP levels.

**Implementation:**
```csharp
// Level 1: In-Service Cache (IMemoryCache)
public async Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct)
{
    // Check cache first
    if (_cache.TryGetValue(CacheKeys.Students.ById(id), out Student? cached))
        return Result<StudentDto>.Success(ToDto(cached));
    
    // Fetch from store, then cache
    var student = _students.TryGetValue(id, out var s) ? s : null;
    if (student != null)
        _cache.Set(CacheKeys.Students.ById(id), student, _cacheOptions);
}

// Level 2: HTTP Output Cache (removed for real-time updates)
// Initially had [OutputCache], but removed to ensure UI sees changes immediately
```

**Rationale:**
- ✅ **Performance**: Reduce redundant queries
- ✅ **Scalability**: Handle more concurrent users
- ✅ **Consistency**: Cache invalidation on writes

**Trade-off:**
- Prioritized **correctness** over **speed** by removing HTTP cache
- Users see updates immediately at cost of slightly more CPU

---

### 5. **Idempotency Support**

**Decision:** Support idempotent POST requests with `Idempotency-Key` header.

**Usage:**
```http
POST /api/v1/students
Content-Type: application/json
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000

{
  "name": "John Doe",
  "email": "john@example.com"
}
```

**Rationale:**
- ✅ **Reliability**: Network failures won't create duplicates
- ✅ **User Experience**: Safe to retry without side effects
- ✅ **Best Practice**: Follows Stripe, Twilio, AWS patterns

**Implementation:**
```csharp
public class IdempotencyMiddleware
{
    // Cache response for 24 hours
    // If same key comes again, return cached response
    // No duplicate resource created
}
```

---

### 6. **Cascade Delete**

**Decision:** When deleting Student/Course, automatically delete related Enrollments.

**Implementation:**
```csharp
[HttpDelete("{id:guid}")]
public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
{
    // First: Delete all enrollments for this student
    await _enrollments.DeleteByStudentAsync(id, ct);
    
    // Then: Delete the student
    var result = await _students.DeleteAsync(id, ct);
    return this.ToActionResult(result);
}
```

**Rationale:**
- ✅ **Data Integrity**: No orphaned enrollments showing "N/A"
- ✅ **User Experience**: Clean, predictable behavior
- ✅ **Consistency**: Mimics SQL `ON DELETE CASCADE`

**Alternative Considered:**
- Return error if student has enrollments (too restrictive)
- Leave orphaned enrollments (confusing for users)

---

### 7. **Async/Await with CancellationToken**

**Decision:** All I/O operations are async with `CancellationToken` support.

**Example:**
```csharp
// ✅ DO: Accept CancellationToken
public async Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct)
{
    await _httpClient.GetAsync(url, ct);  // Can be cancelled
}

// ❌ DON'T: Ignore cancellation
public async Task<Result<StudentDto>> GetByIdAsync(Guid id)
{
    await _httpClient.GetAsync(url);  // No cancellation support
}
```

**Rationale:**
- ✅ **Responsiveness**: Cancel long-running operations
- ✅ **Resource Management**: Free up threads when client disconnects
- ✅ **Scalability**: Handle more concurrent requests

---

### 8. **Service Lifetime Choices**

**Decision:** Services registered with appropriate lifetimes.

```csharp
// Singleton: One instance for app lifetime (safe because in-memory store)
builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddSingleton<IStudentService, StudentService>();

// Scoped: One instance per request
builder.Services.AddScoped<DataSeeder>();

// Transient: New instance every time
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
```

**Rationale:**
- Services use `ConcurrentDictionary` (thread-safe) → safe as Singleton
- Singleton = best performance (no allocations per request)
- If we used EF Core DbContext, would need Scoped

---

### 9. **XML Documentation for Swagger**

**Decision:** Use XML comments on all public APIs.

```csharp
/// <summary>
/// Creates a new student.
/// </summary>
/// <remarks>
/// This endpoint supports idempotency. Include an `Idempotency-Key` header.
/// </remarks>
[HttpPost]
public async Task<ActionResult<StudentDto>> Create(...)
```

**Rationale:**
- ✅ **API Documentation**: Auto-generated, always up-to-date
- ✅ **Developer Experience**: Clear expectations in Swagger UI
- ✅ **Examples**: Consumers know how to use the API

---

### 10. **Global Exception Handling**

**Decision:** Use middleware instead of try-catch in every controller.

```csharp
public class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            // Log error, return RFC 7807 ProblemDetails
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**Rationale:**
- ✅ **DRY**: Single place for error handling
- ✅ **Consistency**: All errors formatted the same way
- ✅ **Separation**: Controllers focus on business logic

---

## 📚 API Documentation

### Base URL
```
Development: https://localhost:5225/api/v1
Production: https://your-domain.com/api/v1
```

### Interactive Documentation
Visit **https://localhost:5225/swagger** when running locally.

### Authentication
Currently no authentication (demo purposes). 

**Production:** Add JWT Bearer tokens:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

### Endpoints

#### Students
| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/students?page=1&pageSize=10` | List students | `PagedResult<StudentDto>` |
| GET | `/students/{id}` | Get by ID | `StudentDto` |
| POST | `/students` | Create student | `StudentDto` |
| PUT | `/students/{id}` | Update student | `StudentDto` |
| DELETE | `/students/{id}` | Delete student | `204 No Content` |

#### Courses
| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/courses?page=1&pageSize=10` | List courses | `PagedResult<CourseDto>` |
| GET | `/courses/{id}` | Get by ID | `CourseDto` |
| POST | `/courses` | Create course | `CourseDto` |
| PUT | `/courses/{id}` | Update course | `204 No Content` |
| DELETE | `/courses/{id}` | Delete course | `204 No Content` |

#### Enrollments
| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/enrollments?page=1&pageSize=10` | List enrollments | `PagedResult<EnrollmentDto>` |
| GET | `/enrollments/by-student/{studentId}` | Get by student | `EnrollmentDto[]` |
| GET | `/enrollments/by-course/{courseId}` | Get by course | `EnrollmentDto[]` |
| POST | `/enrollments` | Create enrollment | `EnrollmentDto` |
| DELETE | `/enrollments/{id}` | Delete enrollment | `204 No Content` |

---

## ⚙️ Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "DataSeeding": {
    "Enabled": false  // false in production
  }
}
```

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DataSeeding": {
    "Enabled": true  // Enable for development
  }
}
```

### Environment Variables
```bash
# Override settings
DOTNET_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5225;http://localhost:5224
DataSeeding__Enabled=true
```

---

## 🛠️ Development

### Adding a New Entity

**1. Create Domain Model** (`Domain/Models/YourEntity.cs`):
```csharp
public sealed class YourEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
}
```

**2. Create DTOs** (`Contracts/DTOs/YourEntityDtos.cs`):
```csharp
public readonly record struct YourEntityDto(Guid Id, string Name);
public sealed class CreateYourEntityDto
{
    [Required] public string Name { get; init; } = default!;
}
```

**3. Create Service Interface** (`Application/Interfaces/IYourEntityService.cs`):
```csharp
public interface IYourEntityService
{
    Task<Result<PagedResult<YourEntityDto>>> QueryAsync(PagingQuery query, CancellationToken ct);
    Task<Result<YourEntityDto>> CreateAsync(CreateYourEntityDto dto, CancellationToken ct);
}
```

**4. Implement Service** (`Application/Services/YourEntityService.cs`):
```csharp
internal sealed class YourEntityService : IYourEntityService
{
    private readonly ConcurrentDictionary<Guid, YourEntity> _entities = new();
    // ... implementation
}
```

**5. Create Controller** (`Presentation/Controllers/YourEntitiesController.cs`):
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class YourEntitiesController : ControllerBase
{
    // ... actions
}
```

**6. Register in DI** (`Program.cs`):
```csharp
builder.Services.AddSingleton<IYourEntityService, YourEntityService>();
```

---

## 🧪 Testing

### Run All Tests
```bash
cd ../..  # Go to solution root
dotnet test
```

### Test Coverage
- **26 unit tests** covering:
  - Service CRUD operations
  - Pagination logic
  - Validation scenarios
  - Idempotency middleware
  - Error handling

### Writing Tests
```csharp
public class YourServiceTests
{
    [Fact]
    public async Task Create_Succeeds_WhenValid()
    {
        // Arrange
        var service = new YourService(_cache);
        var dto = new CreateDto { Name = "Test" };
        
        // Act
        var result = await service.CreateAsync(dto, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test", result.Value.Name);
    }
}
```

---

## 🚀 Deployment

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "Lms.Api.dll"]
```

### Environment Setup
```bash
# Set production environment
export ASPNETCORE_ENVIRONMENT=Production

# Disable data seeding
export DataSeeding__Enabled=false

# Configure HTTPS
export ASPNETCORE_Kestrel__Certificates__Default__Path=/path/to/cert.pfx
export ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword
```

---

## 📖 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Result Pattern](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [API Versioning](https://github.com/dotnet/aspnet-api-versioning)

---

## 🤝 Contributing

When adding features:
1. Follow existing architecture patterns
2. Add unit tests
3. Update XML documentation
4. Test with Swagger UI
5. Update this README if adding major features

---

<div align="center">

**Built with best practices to demonstrate real-world .NET development**

</div>

