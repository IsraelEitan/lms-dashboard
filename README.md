# LMS Dashboard

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.5-3178C6)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

A modern, production-ready Learning Management System built with **ASP.NET Core 9** (backend) and **React 18 + TypeScript** (frontend). This project demonstrates enterprise-level architecture, best practices, and scalability patterns suitable for real-world applications.

---

## Project Purpose & Developer Notes

> **Why I Built It This Way**

While this started as an assignment, I wanted to showcase a **complete, professional solution** that goes beyond basic requirements. This is the kind of architecture and code quality I deliver in production environments.

**Key Philosophy:**
- This is a demo with in-memory storage, but the **architecture is production-ready**
- Every pattern here (Result, Idempotency, Clean Architecture, Structured Logging) I've used in real systems handling millions of users
- The goal isn't just to "make it work" - it's to demonstrate **how I think about scalability, maintainability, and professional development**

**What This Demonstrates:**

**Clean Architecture** - Separation of concerns with distinct layers (Domain, Application, Infrastructure, Presentation)

**Scalability Patterns** - Multi-layer caching, pagination, async operations, distributed-system readiness

**Production-Ready Features** - Structured logging with Serilog, error handling, validation, idempotency, cascade deletes

**Modern UI/UX** - Responsive design, custom modals, real-time updates, professional interactions

**Best Practices** - SOLID principles, DRY, type safety, comprehensive documentation, unit testing

**Professional Tooling** - Swagger/OpenAPI, hot reload, file-based logging, mock S3 integration

**The Trade-off:**
Some patterns might seem like overkill for a demo (idempotency, multiple cache layers, ADRs), but they show my understanding of enterprise systems. In a real scenario, I'd scale this to use Entity Framework Core, Redis for distributed caching, RabbitMQ for async operations, and actual AWS S3 for file storage.

---

## 📋 Table of Contents

- [Features](#-features)
- [Architecture Overview](#️-architecture-overview)
- [Tech Stack](#️-tech-stack)
- [Quick Start](#-quick-start)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Architecture Decision Records](#-architecture-decision-records-adrs)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Additional Documentation](#-additional-documentation)

---

## ✨ Features

### Core Functionality
- **Student Management** - CRUD operations with validation
- **Course Catalog** - Manage courses with code, title, description
- **Enrollment System** - Assign students to courses with referential integrity
- **Dashboard** - Real-time statistics with auto-refresh
- **Reports** - Students per course analytics with S3 export
- **Pagination** - Efficient data browsing with page navigation

### Backend Enterprise Features
- **Structured Logging** - Serilog with configurable file output, daily rotation, retention policies
- **AWS S3 Mock Interface** - Simulated cloud storage for export operations (production-ready interface)
- **Idempotency** - Prevent duplicate operations on network retries
- **Service-Level Caching** - Fast data access with MemoryCache
- **Global Exception Handling** - Consistent RFC 7807 error responses
- **API Versioning** - Future-proof API design (`/api/v1/...`)
- **Rate Limiting** - 100 requests/second with queuing
- **CORS Configuration** - Secure cross-origin requests
- **Cascade Delete** - Maintain referential integrity automatically
- **Data Seeding** - Quick development setup with sample data
- **Result Pattern** - Type-safe error handling without exceptions
- **Swagger/OpenAPI** - Interactive API documentation

### Frontend UI/UX Excellence
- **Modern, Responsive Design** - Mobile-first, works on all devices
- **Custom Confirmation Dialogs** - Professional modals matching app theme
- **Loading States** - Clear feedback during async operations
- **Error Handling** - User-friendly error messages
- **Real-time Updates** - Instant UI refresh after CRUD operations
- **Smooth Animations** - Professional transitions and interactions
- **TypeScript** - 100% type coverage for safety
- **Component Architecture** - Reusable, testable components

---

## 🏗️ Architecture Overview

### Clean Architecture Layers

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────┐  │
│  │ Controllers│  │ Middleware │  │ Result Extension│  │
│  └────────────┘  └────────────┘  └─────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
┌───────────────────────┴─────────────────────────────────┐
│                  Application Layer                       │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐ │
│  │  Services   │  │  Interfaces  │  │  DTOs/Contracts│ │
│  └─────────────┘  └──────────────┘  └────────────────┘ │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
┌───────────────────────┴─────────────────────────────────┐
│                     Domain Layer                         │
│  ┌────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   Models   │  │  Validation │  │  Business Logic │  │
│  └────────────┘  └─────────────┘  └─────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ all layers depend on
┌───────────────────────┴─────────────────────────────────┐
│                Infrastructure Layer                      │
│  ┌────────────┐  ┌──────────┐  ┌───────────────────┐   │
│  │ Data Store │  │  Caching │  │  Cross-Cutting    │   │
│  │(In-Memory) │  │ (Memory) │  │  Concerns         │   │
│  └────────────┘  └──────────┘  └───────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

**Key Principles:**
- **Dependency Inversion**: High-level modules don't depend on low-level modules
- **Separation of Concerns**: Each layer has a single responsibility
- **Testability**: All dependencies are injected via interfaces
- **Flexibility**: Easy to swap implementations (e.g., in-memory → database)

### Frontend Component Architecture

```
App (routing)
  └── Layout (sidebar + content area)
      ├── Dashboard (statistics cards with real-time updates)
      ├── Courses Page
      │   ├── CourseList (paginated table)
      │   ├── CourseForm (create/edit modal)
      │   └── ConfirmDialog (custom delete confirmation)
      ├── Students Page
      │   ├── StudentList (paginated table)
      │   ├── StudentForm (create/edit modal)
      │   └── ConfirmDialog (custom delete confirmation)
      ├── Enrollments Page
      │   ├── EnrollmentList (paginated table)
      │   ├── EnrollmentForm (enroll modal)
      │   └── ConfirmDialog (custom delete confirmation)
      └── Reports Page
          ├── Summary cards (totals with analytics)
          ├── Students per course table with enrollment levels
          └── Export to S3 button
```

---

## 🛠️ Tech Stack

### Backend
| Technology | Purpose | Why? |
|------------|---------|------|
| ASP.NET Core 9 | Web Framework | Modern, performant, cross-platform |
| C# 12 | Language | Latest features, strong typing |
| Serilog | Logging | Structured logging, file output, configurable |
| Result Pattern | Error Handling | Type-safe, no exceptions for flow control |
| MemoryCache | Caching | Fast in-memory data access |
| Swagger/OpenAPI | Documentation | Interactive API testing |
| xUnit + Moq | Testing | Industry standard, easy mocking |

### Frontend
| Technology | Purpose | Why? |
|------------|---------|------|
| React 18 | UI Library | Component-based, hooks, efficient |
| TypeScript 5.5 | Language | Type safety, better developer experience |
| Vite 5 | Build Tool | Lightning-fast HMR, modern bundling |
| React Router 6 | Routing | Client-side navigation |
| Axios | HTTP Client | Interceptors, robust error handling |
| CSS3 Variables | Styling | Custom, performant, themeable |

---

## 🚀 Quick Start

### Prerequisites
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **Git** - [Download](https://git-scm.com/)

### Option 1: Automated Setup (Windows)

```powershell
# Clone the repository
git clone <repository-url>
cd LmsDashboard

# Run the startup script (starts both backend + frontend)
.\start-app.ps1
```

The script will:
- ✅ Build the backend
- ✅ Start API on http://localhost:5225
- ✅ Install frontend dependencies
- ✅ Start UI on http://localhost:5173
- ✅ Seed sample data (15 students, 12 courses, 13 enrollments)

### Option 2: Manual Setup

#### Backend
```bash
cd src/Lms.Api
dotnet restore
dotnet run
```

**Backend URLs:**
- API: http://localhost:5225
- Swagger: http://localhost:5225/swagger
- Health Check: http://localhost:5225/healthz

#### Frontend
```bash
cd client
npm install
npm run dev
```

**Frontend URL:** http://localhost:5173

---

## 📁 Project Structure

```
LmsDashboard/
├── src/
│   └── Lms.Api/                      # Backend API (.NET 9)
│       ├── Application/              # Business logic layer
│       │   ├── Interfaces/           # Service contracts (IStudentService, IS3Service)
│       │   └── Services/             # Service implementations (includes MockS3Service)
│       ├── Contracts/DTOs/           # API contracts (DTOs)
│       ├── Domain/Models/            # Domain entities
│       ├── Infrastructure/           # Cross-cutting concerns
│       │   ├── Attributes/           # Custom attributes (Idempotency)
│       │   ├── Data/                 # Data seeding
│       │   ├── Extensions/           # Helper extensions
│       │   └── Middleware/           # Custom middleware
│       ├── Presentation/             # API presentation
│       │   ├── Contracts/            # Paging, responses
│       │   └── Controllers/          # API endpoints (Students, Courses, Enrollments, Reports)
│       ├── appsettings.json          # Configuration (logging paths, etc.)
│       ├── Program.cs                # Application entry point with Serilog
│       └── README.md                 # Backend documentation
│
├── logs/                            # Log files (auto-generated, gitignored)
│   ├── lms-dev-20251102.log         # Daily rotating logs in development
│   └── lms-20251102.log             # Production logs
│
├── client/                           # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── components/               # React components
│   │   │   ├── Common/               # Reusable UI (Modal, Alert, etc.)
│   │   │   ├── Courses/              # Course components
│   │   │   ├── Enrollments/          # Enrollment components
│   │   │   ├── Layout/               # Layout components
│   │   │   └── Students/             # Student components
│   │   ├── pages/                    # Page components (routes)
│   │   ├── services/                 # API service (Axios)
│   │   ├── styles/                   # CSS styles
│   │   └── types/                    # TypeScript types
│   ├── package.json
│   └── README.md                     # Frontend documentation
│
├── tests/
│   └── Lms.UnitTests/                # Unit tests (26 tests)
│
├── docs/                             # Additional documentation
├── start-app.ps1                     # Startup script
└── README.md                         # This file
```

**Detailed Documentation:**
- **[Backend README](src/Lms.Api/README.md)** - API architecture, ADRs, development guide
- **[Frontend README](client/README.md)** - Component architecture, design decisions

---

## 📚 API Documentation

### Base URL
**Development:** `http://localhost:5225/api/v1`

### Interactive Documentation
Visit **http://localhost:5225/swagger** when the API is running for full interactive documentation.

### Quick Reference

#### Students
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/students?page=1&pageSize=10` | List students (paginated) |
| GET | `/students/{id}` | Get student by ID |
| POST | `/students` | Create student |
| PUT | `/students/{id}` | Update student |
| DELETE | `/students/{id}` | Delete student + enrollments |

#### Courses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/courses?page=1&pageSize=10` | List courses (paginated) |
| GET | `/courses/{id}` | Get course by ID |
| POST | `/courses` | Create course |
| PUT | `/courses/{id}` | Update course |
| DELETE | `/courses/{id}` | Delete course + enrollments |

#### Enrollments
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/enrollments?page=1&pageSize=10` | List enrollments (paginated) |
| GET | `/enrollments/by-student/{studentId}` | Get student's enrollments |
| GET | `/enrollments/by-course/{courseId}` | Get course's enrollments |
| POST | `/enrollments` | Enroll student in course |
| DELETE | `/enrollments/{id}` | Remove enrollment |

#### Reports
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/reports/dashboard-stats` | Get total counts (students, courses, enrollments) |
| GET | `/reports/students-per-course` | Analytics: students enrolled in each course |
| POST | `/reports/export/students-per-course` | Export report to mock S3, returns file name and URL |
| GET | `/reports/exports` | List all exported reports |
| GET | `/reports/exports/{fileName}` | Download specific export |

### Key Features

**Pagination:**
All list endpoints return:
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "totalCount": 50,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Idempotency:**
Include `Idempotency-Key` header for POST requests:
```bash
curl -X POST http://localhost:5225/api/v1/students \
  -H "Idempotency-Key: unique-key-12345" \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "john@example.com"}'
```

**Error Responses:**
RFC 7807 Problem Details format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Student with ID '...' was not found"
}
```

---

## 🎯 Architecture Decision Records (ADRs)

> These decisions showcase patterns and solutions used in professional software development. While some might be overkill for a demo, they demonstrate my understanding of production-ready architecture.

---

### ADR-001: Clean Architecture with Layer Separation

**Context**: Need a maintainable, testable codebase that can scale.

**Decision**: Implement Clean Architecture with 4 distinct layers (Domain, Application, Infrastructure, Presentation).

**Rationale**:
- ✅ **Testability**: Each layer can be tested independently
- ✅ **Maintainability**: Changes isolated to specific layers
- ✅ **Flexibility**: Easy to swap implementations (e.g., in-memory → EF Core)
- ✅ **Team Scalability**: Multiple developers can work on different layers

**Trade-offs**:
- ❌ More files and folders (but better organization)
- ❌ Initial learning curve (but pays off long-term)

**Alternatives Considered**:
- Monolithic structure: All code in Controllers (rejected - not scalable)
- N-Tier architecture: Traditional 3-layer (rejected - too coupled)

**Status**: ✅ Implemented

---

### ADR-002: Result Pattern for Error Handling

**Context**: Need predictable error handling without expensive exceptions.

**Decision**: Use `Result<T>` pattern instead of throwing exceptions for business logic failures.

**Example**:
```csharp
// ✅ DO: Return Result for expected failures
public async Task<Result<StudentDto>> CreateAsync(CreateStudentDto dto, CancellationToken ct)
{
    if (_students.Values.Any(s => s.Email == dto.Email))
        return Result<StudentDto>.Failure(Errors.Student.DuplicateEmail);
    
    return Result<StudentDto>.Success(studentDto);
}

// ❌ DON'T: Throw exceptions for validation
public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
{
    if (_students.Values.Any(s => s.Email == dto.Email))
        throw new ValidationException("Email exists");  // ❌ Expensive!
}
```

**Rationale**:
- ✅ **Performance**: Exceptions cause stack unwinding (expensive)
- ✅ **Explicit**: Compiler forces caller to handle success/failure
- ✅ **Type-Safe**: Can't forget to handle errors
- ✅ **Railway-Oriented Programming**: Functional approach

**Status**: ✅ Implemented across all services

---

### ADR-003: In-Memory Data Storage (ConcurrentDictionary)

**Context**: Demo project that needs fast development without database setup.

**Decision**: Use `ConcurrentDictionary<Guid, T>` for data storage instead of a database.

**Rationale**:
- ✅ **Simplicity**: No database installation required
- ✅ **Fast**: Microsecond-level latency
- ✅ **Focus**: Demonstrates architecture, not database operations
- ✅ **Portable**: Works on any OS where .NET runs
- ✅ **Thread-Safe**: Built-in concurrency support

**Production Alternative**:
```csharp
// For production, replace with:
services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(connectionString));
// Or MongoDB, or Dapper for raw SQL performance
```

**Trade-offs**:
- ❌ Data lost on restart (acceptable for demo)
- ❌ Not scalable across multiple instances (would need Redis)

**Migration Path**: Services implement interfaces → easy to swap with EF Core later

**Status**: ✅ Implemented (suitable for assignment purposes)

---

### ADR-004: Service-Level Caching (MemoryCache Only)

**Context**: Need to optimize performance while ensuring UI sees real-time updates.

**Decision**: Implement caching only at service level using `IMemoryCache`.

**Evolution**:
- ✅ Initially added `[OutputCache]` on HTTP endpoints for performance
- ❌ Discovered it prevented UI from seeing new items immediately
- ✅ Removed HTTP cache, kept service-level cache
- ✅ **Prioritized correctness over speed**

**Implementation**:
```csharp
public async Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct)
{
    if (_cache.TryGetValue(CacheKeys.Students.ById(id), out Student? cached))
        return Result<StudentDto>.Success(ToDto(cached));
    
    // Fetch from store, then cache
}
```

**Rationale**:
- ✅ **Performance**: Reduce CPU for repeated queries
- ✅ **Correctness**: Users see updates immediately
- ✅ **Cache Invalidation**: Clear cache on write operations

**Status**: ✅ Implemented (service-level only)

---

### ADR-005: Idempotency Support for POST Requests

**Context**: Network failures can cause duplicate requests, creating unwanted duplicates.

**Decision**: Support idempotent POST requests using `Idempotency-Key` header.

**How It Works**:
1. Client sends request with unique `Idempotency-Key`
2. Server caches response for 24 hours
3. If same key comes again, return cached response
4. No duplicate resource created

**Rationale**:
- ✅ **Reliability**: Safe to retry failed requests
- ✅ **User Experience**: No accidental duplicates
- ✅ **Industry Standard**: Used by Stripe, Twilio, AWS, PayPal
- ✅ **Distributed Systems**: Essential for microservices

**Trade-offs**:
- ❌ Increased memory usage (caching responses)
- ❌ 24-hour cache window (configurable)

**Status**: ✅ Implemented

---

### ADR-006: Cascade Delete Pattern

**Context**: Deleting a Student/Course leaves orphaned Enrollments showing "N/A".

**Decision**: Automatically delete related Enrollments when deleting a Student or Course.

**Implementation**:
```csharp
[HttpDelete("{id:guid}")]
public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
{
    // Step 1: Delete all enrollments
    await _enrollments.DeleteByStudentAsync(id, ct);
    
    // Step 2: Delete the student
    var result = await _students.DeleteAsync(id, ct);
    return this.ToActionResult(result);
}
```

**Rationale**:
- ✅ **Data Integrity**: No orphaned records
- ✅ **User Experience**: Clean, predictable behavior
- ✅ **Consistency**: Mimics SQL `ON DELETE CASCADE`

**Alternatives Considered**:
- Return error if enrollments exist (too restrictive)
- Leave orphans (confusing UX)

**Status**: ✅ Implemented

---

### ADR-007: Async/Await with CancellationToken

**Context**: Need responsive API that can cancel long-running operations.

**Decision**: All I/O operations use `async/await` with `CancellationToken` support.

**Example**:
```csharp
// ✅ DO: Accept CancellationToken
public async Task<Result<PagedResult<StudentDto>>> QueryAsync(
    PagingQuery query, 
    CancellationToken ct)  // ✅ Can be cancelled
{
    await Task.Delay(100, ct);  // Respects cancellation
}
```

**Rationale**:
- ✅ **Responsiveness**: Cancel operations when client disconnects
- ✅ **Resource Management**: Free up threads immediately
- ✅ **Scalability**: Handle more concurrent requests
- ✅ **Best Practice**: Required for production .NET

**Status**: ✅ Implemented throughout application

---

### ADR-008: Service Lifetime Strategy (Singleton)

**Context**: Need to choose appropriate DI lifetimes for services.

**Decision**: Register services as **Singleton** because they use thread-safe in-memory storage.

```csharp
// ✅ Singleton: One instance for app lifetime
builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
```

**Rationale**:
- ✅ **Performance**: No allocations per request (fastest)
- ✅ **Thread-Safety**: `ConcurrentDictionary` is thread-safe
- ✅ **Memory Efficiency**: Single instance shared

**If using EF Core**:
```csharp
// ❌ DON'T use Singleton with DbContext!
builder.Services.AddDbContext<LmsDbContext>();  // Scoped by default
```

**Status**: ✅ Implemented (appropriate for in-memory storage)

---

### ADR-009: XML Documentation for API Discoverability

**Context**: API consumers need clear documentation.

**Decision**: Use XML comments on all public APIs to auto-generate Swagger documentation.

**Rationale**:
- ✅ **Developer Experience**: Clear API expectations in Swagger UI
- ✅ **Always Up-to-Date**: Docs generated from code
- ✅ **Examples**: Shows request/response formats

**Status**: ✅ Implemented on all controllers

---

### ADR-010: Global Exception Handling Middleware

**Context**: Need consistent error responses across all endpoints.

**Decision**: Use middleware to catch all unhandled exceptions.

**Rationale**:
- ✅ **DRY**: Single place for error handling
- ✅ **Consistency**: All errors formatted the same way (RFC 7807)
- ✅ **Separation**: Controllers focus on business logic

**Status**: ✅ Implemented

---

### ADR-011: Data Seeding with Configuration Toggle

**Context**: Need sample data for development, but not in production.

**Decision**: Implement configurable data seeding service.

**Configuration**:
```json
// appsettings.Development.json
{
  "DataSeeding": {
    "Enabled": true  // ✅ Enabled in development
  }
}

// appsettings.json (production)
{
  "DataSeeding": {
    "Enabled": false  // ❌ Disabled
  }
}
```

**Seeded Data**: 15 students, 12 courses, 13 enrollments (idempotent - won't duplicate if data exists)

**Rationale**:
- ✅ **Developer Experience**: Start app with data immediately
- ✅ **Safety**: Can't accidentally seed production

**Status**: ✅ Implemented

---

### ADR-012: Custom Delete Confirmation Dialog

**Context**: Native browser `confirm()` looks unprofessional.

**Decision**: Create custom `ConfirmDialog` component matching app's look and feel.

**Features**:
- 🎨 Styled consistently with create/edit modals
- ⚠️ Icon-based visual feedback
- 📝 Detailed message with entity name
- 🔄 Loading state during delete

**Rationale**:
- ✅ **Professional UX**: Matches app design system
- ✅ **User Safety**: Clear consequences of action
- ✅ **Reusability**: Used across Students, Courses, Enrollments

**Comparison**:
```typescript
// ❌ Native (ugly, blocking)
if (window.confirm('Delete?')) { ... }

// ✅ Custom (beautiful, branded)
<ConfirmDialog title="Delete Student" ... />
```

**Status**: ✅ Implemented

---

### ADR-013: Structured Logging with Serilog

**Context**: Need production-grade logging for debugging, monitoring, and compliance.

**Decision**: Implement Serilog with file-based logging, configurable paths, and daily rotation.

**Implementation**:
```json
// appsettings.json - Configure log location and behavior
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": { "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}" }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/lms-.log",  // Configurable location
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

**Features**:
- Configurable log file paths via appsettings
- Daily log rotation with automatic cleanup
- Different settings for Development vs Production
- Structured logging with proper severity levels
- No emoji icons (clean, parseable output)

**Rationale**:
- **Debugging**: Comprehensive logging at all layers (controllers, services)
- **Monitoring**: File-based logs for centralized log aggregation (e.g., ELK, Splunk)
- **Compliance**: Audit trails for all operations
- **Performance**: Async logging doesn't block requests

**Production Path**: Logs can be sent to Azure Application Insights, AWS CloudWatch, or any centralized logging service

**Status**: ✅ Implemented across all controllers and services

---

### ADR-014: Mock S3 Interface for Cloud Storage

**Context**: Need to demonstrate cloud storage integration without AWS credentials.

**Decision**: Implement `IS3Service` interface with `MockS3Service` implementation.

**Implementation**:
```csharp
public interface IS3Service
{
    Task<Result> UploadFileAsync(string bucketName, string key, string content, string contentType, CancellationToken ct);
    Task<Result<string>> DownloadFileAsync(string bucketName, string key, CancellationToken ct);
    Task<Result> DeleteFileAsync(string bucketName, string key, CancellationToken ct);
    Task<Result<IReadOnlyList<string>>> ListFilesAsync(string bucketName, string? prefix, CancellationToken ct);
}

// Mock implementation stores files in-memory
// Production implementation would use AWSSDK.S3
```

**Use Case**: Reports controller exports "Students Per Course" data to mock S3

**Rationale**:
- **Production-Ready Interface**: Easy to swap with real AWS S3 SDK
- **No External Dependencies**: Works without AWS credentials
- **Demonstrates Understanding**: Shows knowledge of cloud storage patterns
- **Logging**: All S3 operations are logged

**Production Migration**:
```csharp
// Replace with actual AWS implementation
builder.Services.AddSingleton<IS3Service, AwsS3Service>();
```

**Status**: ✅ Implemented with full logging

---

### Summary: Why These Decisions?

**This is an assignment**, but I wanted to demonstrate:

1. 🎓 **Knowledge of best practices** used in real-world enterprise applications
2. 🏗️ **Understanding of scalable architecture** patterns
3. 🔍 **Awareness of production considerations** (caching, error handling, idempotency)
4. 📚 **Ability to make informed trade-offs** between simplicity and robustness
5. 💼 **Professional development skills** beyond just "making it work"

**The goal**: Show that while this is a demo with in-memory storage, I understand how to architect systems that could scale to millions of users with proper database, distributed caching, message queues, cloud storage, structured logging, and microservices.

---

## 🧪 Testing

### Run Unit Tests
```bash
dotnet test
```

**Test Coverage**:
- **26 Unit Tests** with xUnit and Moq
- Student service CRUD operations
- Course service CRUD operations
- Enrollment service operations
- Idempotency middleware
- Pagination logic
- Validation scenarios

**Example Test**:
```csharp
[Fact]
public async Task CreateStudent_Succeeds_WhenValidData()
{
    var service = new StudentService(_cache);
    var dto = new CreateStudentDto("John Doe", "john@example.com");
    
    var result = await service.CreateAsync(dto, CancellationToken.None);
    
    Assert.True(result.IsSuccess);
    Assert.Equal("John Doe", result.Value.Name);
}
```

---

## 📦 Deployment

### Backend (API)
```bash
cd src/Lms.Api
dotnet publish -c Release -o ./publish
```

Deploy `publish` folder to:
- Azure App Service
- AWS Elastic Beanstalk
- Docker container
- IIS

### Frontend
```bash
cd client
npm run build
```

Deploy `dist` folder to:
- Vercel
- Netlify
- Azure Static Web Apps
- Nginx/Apache

---

## 📖 Additional Documentation

- **[Backend README](src/Lms.Api/README.md)** - Detailed API architecture, development guide
- **[Frontend README](client/README.md)** - Component architecture, design decisions
- **[Testing Guide](docs/TESTING_GUIDE.md)** - Testing strategies and results

---

## 🎓 What This Project Demonstrates

My ability to:

1. **Design Clean Architecture** - Proper separation of concerns, SOLID principles
2. **Implement Enterprise Patterns** - Result pattern, caching, idempotency, structured logging
3. **Build Production-Ready APIs** - Versioning, error handling, documentation, monitoring
4. **Create Modern UIs** - React best practices, TypeScript, responsive design
5. **Write Maintainable Code** - Clear naming, documentation, testing
6. **Think About Scale** - Caching strategies, pagination, async operations, cloud storage
7. **Follow Best Practices** - Code organization, security considerations, logging
8. **Deliver Complete Solutions** - From architecture to deployment to observability

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

<div align="center">

**Built with ❤️ to demonstrate real-world development skills**

⭐ Star this repo if you found it helpful!

[Backend Docs](src/Lms.Api/README.md) • [Frontend Docs](client/README.md) • [Report Issues](../../issues)

</div>
