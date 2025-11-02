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
/// Provides student management operations including registration, retrieval, and deletion.
/// Implements an in-memory repository pattern with caching for optimal performance.
/// Thread-safe implementation using ConcurrentDictionary for concurrent access scenarios.
/// </summary>
internal sealed class StudentService : IStudentService
{
  private readonly ConcurrentDictionary<Guid, Student> _students = new();
  private readonly IMemoryCache _cache;

  private static readonly MemoryCacheEntryOptions _listOpts =
      new MemoryCacheEntryOptions().SetAbsoluteExpiration(Constants.CacheDurations.ShortList);

  private static readonly MemoryCacheEntryOptions _itemOpts =
      new MemoryCacheEntryOptions().SetAbsoluteExpiration(Constants.CacheDurations.Item);

  /// <summary>
  /// Initializes a new instance of the <see cref="StudentService"/> class.
  /// </summary>
  /// <param name="cache">The memory cache for optimizing read operations.</param>
  public StudentService(IMemoryCache cache)
  {
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
  }

  public Task<Result<PagedResult<StudentDto>>> QueryAsync(PagingQuery query, CancellationToken ct)
  {
    var list = _cache.TryGetValue(Constants.CacheKeys.Students.All, out IReadOnlyList<Student>? cached)
        ? cached
        : _students.Values.OrderBy(s => s.Name).ToList().AsReadOnly();

    list ??= Array.Empty<Student>();

    // Apply search filter
    var filtered = QueryHelper<Student>.ApplySearch(
      list,
      query,
      (student, searchTerm) =>
        (student.Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (student.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
    );

    // Apply sorting
    var sorted = QueryHelper<Student>.ApplySort(
      filtered,
      query,
      (student, key) => key switch
      {
        "name" => student.Name,
        "email" => student.Email,
        _ => student.Id
      }
    );

    // Apply pagination
    var payload = QueryHelper<Student>.ApplyPagination(
      sorted,
      query,
      student => new StudentDto(student.Id, student.Name, student.Email)
    );

    return Task.FromResult(Result<PagedResult<StudentDto>>.Success(payload));
  }

  public Task<Result<IReadOnlyList<StudentDto>>> GetAllAsync(CancellationToken ct)
  {
    if (!_cache.TryGetValue(Constants.CacheKeys.Students.All, out IReadOnlyList<Student>? list) || list is null)
    {
      list = _students.Values.OrderBy(s => s.Name).ToList().AsReadOnly();
      _cache.Set(Constants.CacheKeys.Students.All, list, _listOpts);
    }

    var dtos = list.Select(s => new StudentDto(s.Id, s.Name, s.Email)).ToList().AsReadOnly();
    return Task.FromResult(Result<IReadOnlyList<StudentDto>>.Success(dtos));
  }

  public Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct)
  {
    if (!_cache.TryGetValue(Constants.CacheKeys.Students.ById(id), out Student? s)
        && _students.TryGetValue(id, out s))
    {
      _cache.Set(Constants.CacheKeys.Students.ById(id), s, _itemOpts);
    }

    if (s is not null)
    {
      var dto = new StudentDto(s.Id, s.Name, s.Email);
      return Task.FromResult(Result<StudentDto>.Success(dto));
    }

    return Task.FromResult(Result<StudentDto>.Failure(Errors.Student.NotFound(id)));
  }

  /// <summary>
  /// Registers a new student after validating required fields and email format.
  /// </summary>
  /// <param name="dto">The student registration data.</param>
  /// <param name="ct">Cancellation token for the operation.</param>
  /// <returns>A result containing the created student DTO or validation errors.</returns>
  public Task<Result<StudentDto>> CreateAsync(CreateStudentDto dto, CancellationToken ct)
  {
    // Validate required fields
    var nameValidation = ValidationHelper.ValidateName(dto.Name);
    if (!nameValidation.IsSuccess)
    {
      return Task.FromResult(Result<StudentDto>.Failure(nameValidation.Error));
    }

    var emailValidation = ValidationHelper.ValidateEmail(dto.Email);
    if (!emailValidation.IsSuccess)
    {
      return Task.FromResult(Result<StudentDto>.Failure(emailValidation.Error));
    }

    // Create new student entity with validated data
    var s = new Student
    {
      Name = dto.Name!.Trim(),
      Email = dto.Email!.Trim()
    };

    // Persist to repository and update cache
    _students[s.Id] = s;
    _cache.Remove(Constants.CacheKeys.Students.All); // Invalidate list cache
    _cache.Set(Constants.CacheKeys.Students.ById(s.Id), s, _itemOpts);

    var studentDto = new StudentDto(s.Id, s.Name, s.Email);
    return Task.FromResult(Result<StudentDto>.Success(studentDto));
  }

  /// <summary>
  /// Updates an existing student's profile information.
  /// </summary>
  /// <param name="id">The student identifier.</param>
  /// <param name="dto">The updated student data.</param>
  /// <param name="ct">Cancellation token for the operation.</param>
  /// <returns>A result containing the updated student DTO or validation/not found errors.</returns>
  public Task<Result<StudentDto>> UpdateAsync(Guid id, UpdateStudentDto dto, CancellationToken ct)
  {
    // Validate required fields
    var nameValidation = ValidationHelper.ValidateName(dto.Name);
    if (!nameValidation.IsSuccess)
    {
      return Task.FromResult(Result<StudentDto>.Failure(nameValidation.Error));
    }

    var emailValidation = ValidationHelper.ValidateEmail(dto.Email);
    if (!emailValidation.IsSuccess)
    {
      return Task.FromResult(Result<StudentDto>.Failure(emailValidation.Error));
    }

    // Check if student exists
    if (!_students.TryGetValue(id, out var student))
    {
      return Task.FromResult(Result<StudentDto>.Failure(Errors.Student.NotFound(id)));
    }

    // Update student entity
    student.Update(dto.Name!.Trim(), dto.Email!.Trim());

    // Update cache
    _cache.Remove(Constants.CacheKeys.Students.All);
    _cache.Set(Constants.CacheKeys.Students.ById(id), student, _itemOpts);

    var studentDto = new StudentDto(student.Id, student.Name, student.Email);
    return Task.FromResult(Result<StudentDto>.Success(studentDto));
  }

  public Task<Result> DeleteAsync(Guid id, CancellationToken ct)
  {
    if (_students.TryRemove(id, out _))
    {
      _cache.Remove(Constants.CacheKeys.Students.All);
      _cache.Remove(Constants.CacheKeys.Students.ById(id));
      return Task.FromResult(Result.Success());
    }

    return Task.FromResult(Result.Failure(Errors.Student.NotFound(id)));
  }
}
