using Lms.Api.Application.Interfaces;
using Lms.Api.Contracts.DTOs;

namespace Lms.Api.Infrastructure.Data;

/// <summary>
/// Seeds initial data for development and testing purposes.
/// </summary>
internal sealed class DataSeeder
{
  private readonly IStudentService _students;
  private readonly ICourseService _courses;
  private readonly IEnrollmentService _enrollments;
  private readonly ILogger<DataSeeder> _logger;

  public DataSeeder(
    IStudentService students,
    ICourseService courses,
    IEnrollmentService enrollments,
    ILogger<DataSeeder> logger)
  {
    _students = students;
    _courses = courses;
    _enrollments = enrollments;
    _logger = logger;
  }

  /// <summary>
  /// Seeds the database with initial data if it's empty.
  /// This method is idempotent - safe to run multiple times.
  /// </summary>
  public async Task SeedAsync(CancellationToken ct = default)
  {
    _logger.LogInformation("Starting data seeding process");

    // Check if data already exists
    var existingStudents = await _students.GetAllAsync(ct);
    var existingCourses = await _courses.GetAllAsync(ct);
    var existingEnrollments = await _enrollments.QueryAsync(new Presentation.Contracts.PagingQuery { PageSize = 1 }, ct);

    if (existingStudents.Value?.Count > 0 || existingCourses.Value?.Count > 0 || existingEnrollments.Value?.Items.Count > 0)
    {
      _logger.LogInformation("Data already exists. Skipping seeding");
      return;
    }

    try
    {
      // Seed Students (15)
      var studentIds = await SeedStudentsAsync(ct);
      _logger.LogInformation("Seeded {Count} students successfully", studentIds.Count);

      // Seed Courses (12)
      var courseIds = await SeedCoursesAsync(ct);
      _logger.LogInformation("Seeded {Count} courses successfully", courseIds.Count);

      // Seed Enrollments (13)
      await SeedEnrollmentsAsync(studentIds, courseIds, ct);
      _logger.LogInformation("Seeded 13 enrollments successfully");

      _logger.LogInformation("Data seeding completed successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during data seeding");
      throw;
    }
  }

  private async Task<List<Guid>> SeedStudentsAsync(CancellationToken ct)
  {
    var students = new List<CreateStudentDto>
    {
      new() { Name = "Shai Zuckerberg", Email = "shai.zuckerberg@university.il" },
      new() { Name = "Dana Muskowitz", Email = "dana.muskowitz@university.il" },
      new() { Name = "Eitan Altman", Email = "eitan.altman@university.il" },
      new() { Name = "Noa Ben-Gates", Email = "noa.ben-gates@university.il" },
      new() { Name = "Omer Nadella", Email = "omer.nadella@university.il" },
      new() { Name = "Liron Sandberg", Email = "liron.sandberg@university.il" },
      new() { Name = "Tamar Jobsman", Email = "tamar.jobsman@university.il" },
      new() { Name = "Yair Brinstein", Email = "yair.brinstein@university.il" },
      new() { Name = "Gal Pichai", Email = "gal.pichai@university.il" },
      new() { Name = "Roni Bezosov", Email = "roni.bezosov@university.il" },
      new() { Name = "Tal Friedmanberg", Email = "tal.friedmanberg@university.il" },
      new() { Name = "Itay Cookman", Email = "itay.cookman@university.il" },
      new() { Name = "Maya Chesky", Email = "maya.chesky@university.il" },
      new() { Name = "Oren Nadavsky", Email = "oren.nadavsky@university.il" },
      new() { Name = "Aviv Kalanik", Email = "aviv.kalanik@university.il" }
    };

    return await SeedEntitiesAsync(
      students,
      async (student, token) => await _students.CreateAsync(student, token)
    );
  }

  private async Task<List<Guid>> SeedCoursesAsync(CancellationToken ct)
  {
    var courses = new List<CreateUpdateCourseDto>
    {
      new() { Code = "CS101", Title = "Introduction to Computer Science", Description = "Fundamentals of programming and computational thinking" },
      new() { Code = "CS201", Title = "Data Structures and Algorithms", Description = "Core data structures and algorithm design" },
      new() { Code = "CS301", Title = "Database Systems", Description = "Relational databases, SQL, and data modeling" },
      new() { Code = "CS401", Title = "Software Engineering", Description = "Software development methodologies and best practices" },
      new() { Code = "MATH101", Title = "Calculus I", Description = "Limits, derivatives, and integrals" },
      new() { Code = "MATH201", Title = "Linear Algebra", Description = "Vector spaces, matrices, and linear transformations" },
      new() { Code = "PHYS101", Title = "Physics I: Mechanics", Description = "Classical mechanics and motion" },
      new() { Code = "ENG101", Title = "English Composition", Description = "Academic writing and critical thinking" },
      new() { Code = "BIO101", Title = "Introduction to Biology", Description = "Cell biology, genetics, and evolution" },
      new() { Code = "CHEM101", Title = "General Chemistry", Description = "Atomic structure, chemical bonding, and reactions" },
      new() { Code = "HIST201", Title = "World History", Description = "Major civilizations and historical events" },
      new() { Code = "PSY101", Title = "Introduction to Psychology", Description = "Human behavior and mental processes" }
    };

    return await SeedEntitiesAsync(
      courses,
      async (course, token) => await _courses.CreateAsync(course, token)
    );
  }

  private async Task<List<Guid>> SeedEntitiesAsync<TDto, TResult>(
    List<TDto> items,
    Func<TDto, CancellationToken, Task<Common.Results.Result<TResult>>> createFunc)
  {
    var ids = new List<Guid>();
    foreach (var item in items)
    {
      var result = await createFunc(item, default);
      if (result.IsSuccess)
      {
        // Extract Id using reflection (works for DTOs with Id property)
        var idProperty = typeof(TResult).GetProperty("Id");
        if (idProperty != null)
        {
          var boxedValue = (object)result.Value!;
          if (idProperty.GetValue(boxedValue) is Guid id)
          {
            ids.Add(id);
          }
        }
      }
    }
    return ids;
  }

  private async Task SeedEnrollmentsAsync(List<Guid> studentIds, List<Guid> courseIds, CancellationToken ct)
  {
    // Create 13 diverse enrollments
    var enrollments = new List<(Guid StudentId, Guid CourseId)>
    {
      (studentIds[0], courseIds[0]),  
      (studentIds[0], courseIds[4]),  
      (studentIds[1], courseIds[0]),  
      (studentIds[1], courseIds[1]),  
      (studentIds[2], courseIds[2]),  
      (studentIds[3], courseIds[5]),  
      (studentIds[4], courseIds[6]),  
      (studentIds[5], courseIds[7]),  
      (studentIds[6], courseIds[0]),  
      (studentIds[7], courseIds[8]),  
      (studentIds[8], courseIds[9]),  
      (studentIds[9], courseIds[10]), 
      (studentIds[10], courseIds[11]) 
    };

    foreach (var (studentId, courseId) in enrollments)
    {
      await _enrollments.AssignAsync(studentId, courseId, ct);
    }
  }
}

