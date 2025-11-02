using System.Collections.Concurrent;
using Lms.Api.Application.Interfaces;
using Lms.Api.Common;
using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Domain.Models;
using Lms.Api.Presentation.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Lms.Api.Application.Services;

/// <summary>
/// Provides course management operations including creation, retrieval, updating, and deletion.
/// Implements an in-memory repository pattern with caching for optimal performance.
/// Thread-safe implementation using ConcurrentDictionary for concurrent access scenarios.
/// </summary>
internal sealed class CourseService : ICourseService
{
  private readonly ConcurrentDictionary<Guid, Course> _courses = new();
  private readonly IMemoryCache _cache;

  private static readonly MemoryCacheEntryOptions _listOpts =
      new MemoryCacheEntryOptions().SetAbsoluteExpiration(Constants.CacheDurations.ShortList);

  private static readonly MemoryCacheEntryOptions _itemOpts =
      new MemoryCacheEntryOptions().SetAbsoluteExpiration(Constants.CacheDurations.Item);

  /// <summary>
  /// Initializes a new instance of the <see cref="CourseService"/> class.
  /// </summary>
  /// <param name="cache">The memory cache for optimizing read operations.</param>
  public CourseService(IMemoryCache cache)
  {
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
  }

  public Task<Result<PagedResult<CourseDto>>> QueryAsync(PagingQuery query, CancellationToken ct)
  {
    var list = _cache.TryGetValue(Constants.CacheKeys.Courses.All, out IReadOnlyList<Course>? cached)
        ? cached
        : _courses.Values.OrderBy(c => c.Code).ToList().AsReadOnly();

    list ??= Array.Empty<Course>();

    // Apply search filter
    var filtered = QueryHelper<Course>.ApplySearch(
      list,
      query,
      (course, searchTerm) =>
        (course.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (course.Code?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
    );

    // Apply sorting
    var sorted = QueryHelper<Course>.ApplySort(
      filtered,
      query,
      (course, key) => key switch
      {
        "code" => course.Code,
        "title" => course.Title,
        _ => course.Id
      }
    );

    // Apply pagination
    var payload = QueryHelper<Course>.ApplyPagination(
      sorted,
      query,
      course => new CourseDto(course.Id, course.Code, course.Title, course.Description)
    );

    return Task.FromResult(Result<PagedResult<CourseDto>>.Success(payload));
  }

  public Task<Result<IReadOnlyList<CourseDto>>> GetAllAsync(CancellationToken ct)
  {
    if (!_cache.TryGetValue(Constants.CacheKeys.Courses.All, out IReadOnlyList<Course>? list) || list is null)
    {
      list = _courses.Values.OrderBy(c => c.Code).ToList().AsReadOnly();
      _cache.Set(Constants.CacheKeys.Courses.All, list, _listOpts);
    }

    var dtos = list.Select(c => new CourseDto(c.Id, c.Code, c.Title, c.Description)).ToList().AsReadOnly();
    return Task.FromResult(Result<IReadOnlyList<CourseDto>>.Success(dtos));
  }

  public Task<Result<CourseDto>> GetByIdAsync(Guid id, CancellationToken ct)
  {
    if (!_cache.TryGetValue(Constants.CacheKeys.Courses.ById(id), out Course? c)
        && _courses.TryGetValue(id, out c))
    {
      _cache.Set(Constants.CacheKeys.Courses.ById(id), c, _itemOpts);
    }

    if (c is not null)
    {
      var dto = new CourseDto(c.Id, c.Code, c.Title, c.Description);
      return Task.FromResult(Result<CourseDto>.Success(dto));
    }

    return Task.FromResult(Result<CourseDto>.Failure(Errors.Course.NotFound(id)));
  }

  /// <summary>
  /// Creates a new course after validating uniqueness and required fields.
  /// </summary>
  /// <param name="dto">The course creation data.</param>
  /// <param name="ct">Cancellation token for the operation.</param>
  /// <returns>A result containing the created course DTO or validation errors.</returns>
  public Task<Result<CourseDto>> CreateAsync(CreateUpdateCourseDto dto, CancellationToken ct)
  {
    // Validate required fields
    var codeValidation = ValidationHelper.ValidateCourseCode(dto.Code);
    if (!codeValidation.IsSuccess)
    {
      return Task.FromResult(Result<CourseDto>.Failure(codeValidation.Error));
    }

    var titleValidation = ValidationHelper.ValidateCourseTitle(dto.Title);
    if (!titleValidation.IsSuccess)
    {
      return Task.FromResult(Result<CourseDto>.Failure(titleValidation.Error));
    }

    // Normalize and validate uniqueness of course code
    var code = dto.Code!.Trim();
    if (_courses.Values.Any(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase)))
    {
      return Task.FromResult(Result<CourseDto>.Failure(Errors.Course.DuplicateCode(code)));
    }

    // Create new course entity with validated data
    var course = new Course
    {
      Code = code,
      Title = dto.Title!.Trim(),
      Description = dto.Description?.Trim()
    };

    // Persist to repository and update cache
    _courses[course.Id] = course;
    _cache.Remove(Constants.CacheKeys.Courses.All); // Invalidate list cache
    _cache.Set(Constants.CacheKeys.Courses.ById(course.Id), course, _itemOpts);

    var resultDto = new CourseDto(course.Id, course.Code, course.Title, course.Description);
    return Task.FromResult(Result<CourseDto>.Success(resultDto));
  }

  /// <summary>
  /// Updates an existing course with new values after validating existence and uniqueness constraints.
  /// </summary>
  /// <param name="id">The unique identifier of the course to update.</param>
  /// <param name="dto">The updated course data.</param>
  /// <param name="ct">Cancellation token for the operation.</param>
  /// <returns>A result indicating success or failure with appropriate error details.</returns>
  public Task<Result> UpdateAsync(Guid id, CreateUpdateCourseDto dto, CancellationToken ct)
  {
    // Verify course exists
    if (!_courses.TryGetValue(id, out var existing))
    {
      return Task.FromResult(Result.Failure(Errors.Course.NotFound(id)));
    }

    // Validate code uniqueness (excluding current course)
    var newCode = dto.Code.Trim();
    if (_courses.Values.Any(x => x.Id != id &&
        string.Equals(x.Code, newCode, StringComparison.OrdinalIgnoreCase)))
    {
      return Task.FromResult(Result.Failure(Errors.Course.DuplicateCode(newCode)));
    }

    // Apply updates using domain model's Update method
    existing.Update(newCode, dto.Title.Trim(), dto.Description?.Trim());
    _courses[id] = existing;
    
    // Invalidate affected cache entries
    _cache.Remove(Constants.CacheKeys.Courses.All);
    _cache.Set(Constants.CacheKeys.Courses.ById(id), existing, _itemOpts);

    return Task.FromResult(Result.Success());
  }

  public Task<Result> DeleteAsync(Guid id, CancellationToken ct)
  {
    if (_courses.TryRemove(id, out _))
    {
      _cache.Remove(Constants.CacheKeys.Courses.All);
      _cache.Remove(Constants.CacheKeys.Courses.ById(id));
      return Task.FromResult(Result.Success());
    }

    return Task.FromResult(Result.Failure(Errors.Course.NotFound(id)));
  }
}
