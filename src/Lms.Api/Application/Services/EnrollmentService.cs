using System.Collections.Concurrent;
using Lms.Api.Application.Interfaces;
using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Domain.Models;
using Lms.Api.Presentation.Contracts;

namespace Lms.Api.Application.Services;

/// <summary>
/// Manages student enrollments in courses with validation for referential integrity.
/// Ensures students and courses exist before creating enrollments and prevents duplicate enrollments.
/// Thread-safe implementation using ConcurrentDictionary for concurrent access scenarios.
/// </summary>
internal sealed class EnrollmentService : IEnrollmentService
{
  private readonly ConcurrentDictionary<Guid, Enrollment> _enrollments = new();
  private readonly ICourseService _courses;
  private readonly IStudentService _students;

  /// <summary>
  /// Initializes a new instance of the <see cref="EnrollmentService"/> class.
  /// </summary>
  /// <param name="courses">The course service for validating course existence.</param>
  /// <param name="students">The student service for validating student existence.</param>
  public EnrollmentService(ICourseService courses, IStudentService students)
  {
    _courses = courses ?? throw new ArgumentNullException(nameof(courses));
    _students = students ?? throw new ArgumentNullException(nameof(students));
  }

  private async Task<EnrollmentDto> ToDto(Enrollment enrollment, CancellationToken ct = default)
  {
    var studentTask = _students.GetByIdAsync(enrollment.StudentId, ct);
    var courseTask = _courses.GetByIdAsync(enrollment.CourseId, ct);
    
    await Task.WhenAll(studentTask, courseTask);
    
    StudentDto? student = studentTask.Result.IsSuccess ? studentTask.Result.Value : (StudentDto?)null;
    CourseDto? course = courseTask.Result.IsSuccess ? courseTask.Result.Value : (CourseDto?)null;
    
    return new EnrollmentDto(
      enrollment.Id,
      enrollment.StudentId,
      enrollment.CourseId,
      enrollment.EnrolledAt,
      student?.Name,
      student?.Email,
      course?.Code,
      course?.Title
    );
  }

  public async Task<Result<PagedResult<EnrollmentDto>>> QueryAsync(PagingQuery query, CancellationToken ct)
  {
    var list = _enrollments.Values.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(query.Search))
    {
      list = list.Where(e =>
          e.CourseId.ToString().Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
          e.StudentId.ToString().Contains(query.Search, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(query.Sort))
    {
      var keys = query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      IOrderedEnumerable<Enrollment> ordered = keys[0].StartsWith('-')
          ? list.OrderByDescending(e => SortKey(e, keys[0][1..]))
          : list.OrderBy(e => SortKey(e, keys[0]));

      foreach (var k in keys.Skip(1))
      {
        ordered = k.StartsWith('-')
            ? ordered.ThenByDescending(e => SortKey(e, k[1..]))
            : ordered.ThenBy(e => SortKey(e, k));
      }

      list = ordered;

      static object? SortKey(Enrollment e, string key) => key switch
      {
        "course" => e.CourseId,
        "student" => e.StudentId,
        "date" => e.EnrolledAt,
        _ => e.Id
      };
    }

    var source = list.ToList();
    var page = Math.Max(1, query.Page);
    var size = Math.Clamp(query.PageSize, 1, 100);
    var total = source.Count;
    var totalPages = (int)Math.Ceiling((double)total / size);
    var pagedEnrollments = source.Skip((page - 1) * size).Take(size).ToList();
    
    var items = new List<EnrollmentDto>();
    foreach (var enrollment in pagedEnrollments)
    {
      items.Add(await ToDto(enrollment, ct));
    }

    var payload = new PagedResult<EnrollmentDto>(
        items, 
        page, 
        size, 
        totalPages, 
        total,
        page > 1,
        page < totalPages
    );
    return Result<PagedResult<EnrollmentDto>>.Success(payload);
  }

  public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetAllAsync(CancellationToken ct)
  {
    var enrollments = _enrollments.Values.ToList();
    var result = new List<EnrollmentDto>();
    
    foreach (var enrollment in enrollments)
    {
      result.Add(await ToDto(enrollment, ct));
    }

    return Result<IReadOnlyList<EnrollmentDto>>.Success(result.AsReadOnly());
  }

  public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetByCourseAsync(Guid courseId, CancellationToken ct)
  {
    var enrollments = _enrollments.Values.Where(e => e.CourseId == courseId).ToList();
    var result = new List<EnrollmentDto>();
    
    foreach (var enrollment in enrollments)
    {
      result.Add(await ToDto(enrollment, ct));
    }

    return Result<IReadOnlyList<EnrollmentDto>>.Success(result.AsReadOnly());
  }

  public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetByStudentAsync(Guid studentId, CancellationToken ct)
  {
    var enrollments = _enrollments.Values.Where(e => e.StudentId == studentId).ToList();
    var result = new List<EnrollmentDto>();
    
    foreach (var enrollment in enrollments)
    {
      result.Add(await ToDto(enrollment, ct));
    }

    return Result<IReadOnlyList<EnrollmentDto>>.Success(result.AsReadOnly());
  }

  /// <summary>
  /// Enrolls a student in a course after validating that both exist and the student isn't already enrolled.
  /// </summary>
  /// <param name="studentId">The unique identifier of the student.</param>
  /// <param name="courseId">The unique identifier of the course.</param>
  /// <param name="ct">Cancellation token for the operation.</param>
  /// <returns>A result containing the created enrollment DTO or validation/business rule errors.</returns>
  public async Task<Result<EnrollmentDto>> AssignAsync(Guid studentId, Guid courseId, CancellationToken ct)
  {
    // Validate course exists (referential integrity)
    var courseResult = await _courses.GetByIdAsync(courseId, ct);
    if (!courseResult.IsSuccess)
    {
      return Result<EnrollmentDto>.Failure(Errors.Enrollment.CourseMissing(courseId));
    }

    // Validate student exists (referential integrity)
    var studentResult = await _students.GetByIdAsync(studentId, ct);
    if (!studentResult.IsSuccess)
    {
      return Result<EnrollmentDto>.Failure(Errors.Enrollment.StudentMissing(studentId));
    }

    // Prevent duplicate enrollments (business rule)
    if (_enrollments.Values.Any(e => e.StudentId == studentId && e.CourseId == courseId))
    {
      return Result<EnrollmentDto>.Failure(Errors.Enrollment.AlreadyEnrolled(studentId, courseId));
    }

    // Create enrollment using factory method
    var enrollment = Enrollment.Create(studentId, courseId);
    _enrollments[enrollment.Id] = enrollment;
    
    var dto = await ToDto(enrollment, ct);
    return Result<EnrollmentDto>.Success(dto);
  }

  public Task<Result> DeleteAsync(Guid enrollmentId, CancellationToken ct)
  {
    if (_enrollments.TryRemove(enrollmentId, out _))
    {
      return Task.FromResult(Result.Success());
    }

    return Task.FromResult(Result.Failure(Errors.Enrollment.NotFound(enrollmentId)));
  }

  public Task<Result> DeleteByStudentAsync(Guid studentId, CancellationToken ct)
  {
    var toRemove = _enrollments.Values.Where(e => e.StudentId == studentId).Select(e => e.Id).ToList();
    foreach (var id in toRemove)
    {
      _enrollments.TryRemove(id, out _);
    }
    return Task.FromResult(Result.Success());
  }

  public Task<Result> DeleteByCourseAsync(Guid courseId, CancellationToken ct)
  {
    var toRemove = _enrollments.Values.Where(e => e.CourseId == courseId).Select(e => e.Id).ToList();
    foreach (var id in toRemove)
    {
      _enrollments.TryRemove(id, out _);
    }
    return Task.FromResult(Result.Success());
  }
}
