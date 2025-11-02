using Lms.Api.Application.Interfaces;
using Lms.Api.Common;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Infrastructure.Attributes;
using Lms.Api.Infrastructure.Extensions;
using Lms.Api.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Lms.Api.Presentation.Controllers;

/// <summary>
/// Manages student directory operations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class StudentsController : ControllerBase
{
  private readonly IStudentService _students;
  private readonly IEnrollmentService _enrollments;
  private readonly ILogger<StudentsController> _logger;

  /// <summary>DI constructor.</summary>
  public StudentsController(IStudentService students, IEnrollmentService enrollments, ILogger<StudentsController> logger)
  {
    _students = students;
    _enrollments = enrollments;
    _logger = logger;
  }

  /// <summary>Gets all students (paged, sortable, searchable).</summary>
  [HttpGet]
  [ProducesResponseType(typeof(PagedResult<StudentDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<PagedResult<StudentDto>>> GetAll([FromQuery] PagingQuery query, CancellationToken ct)
  {
    _logger.LogInformation("[GET] /students - Page: {Page}, PageSize: {PageSize}", query.Page, query.PageSize);
    var result = await _students.QueryAsync(query, ct);
    return this.ToActionResult(result);
  }

  /// <summary>Gets a student by its identifier.</summary>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<StudentDto>> GetById(Guid id, CancellationToken ct)
  {
    _logger.LogInformation("[GET] /students/{Id}", id);
    
    var result = await _students.GetByIdAsync(id, ct);
    
    if (!result.IsSuccess)
    {
      _logger.LogWarning("Student not found: {Id}", id);
    }
    
    return this.ToActionResult(result);
  }

  /// <summary>Creates a new student.</summary>
  /// <remarks>
  /// This endpoint supports idempotency. Include an `Idempotency-Key` header to ensure
  /// that retrying the request with the same key will not create duplicate resources.
  /// </remarks>
  [HttpPost]
  [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
  public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentDto dto, CancellationToken ct)
  {
    _logger.LogInformation("[POST] /students - Creating student: {Name}", dto.Name);
    
    var result = await _students.CreateAsync(dto, ct);
    
    if (!result.IsSuccess)
    {
      _logger.LogWarning("Failed to create student: {Name}, Error: {Error}", dto.Name, result.Error);
      return this.ToActionResult(result);
    }

    _logger.LogInformation("Student created successfully: {Id}, Name: {Name}", result.Value.Id, result.Value.Name);
    
    var routeValues = new { id = result.Value.Id };
    return CreatedAtAction(nameof(GetById), routeValues, result.Value);
  }

  /// <summary>Updates an existing student.</summary>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<StudentDto>> Update(Guid id, [FromBody] UpdateStudentDto dto, CancellationToken ct)
  {
    _logger.LogInformation("[PUT] /students/{Id} - Updating student: {Name}", id, dto.Name);
    
    var result = await _students.UpdateAsync(id, dto, ct);
    
    if (result.IsSuccess)
    {
      _logger.LogInformation("Student updated successfully: {Id}", id);
    }
    else
    {
      _logger.LogWarning("Failed to update student: {Id}, Error: {Error}", id, result.Error);
    }
    
    return this.ToActionResult(result);
  }

  /// <summary>Deletes a student and all their enrollments.</summary>
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
  {
    _logger.LogInformation("[DELETE] /students/{Id} - Deleting student and cascading enrollments", id);
    
    // First delete all enrollments for this student (cascade delete)
    var enrollmentResult = await _enrollments.DeleteByStudentAsync(id, ct);
    if (enrollmentResult.IsSuccess)
    {
      _logger.LogDebug("Cascade deleted enrollments for student: {Id}", id);
    }
    
    // Then delete the student
    var result = await _students.DeleteAsync(id, ct);
    
    if (result.IsSuccess)
    {
      _logger.LogInformation("Student deleted successfully: {Id}", id);
    }
    else
    {
      _logger.LogWarning("Failed to delete student: {Id}, Error: {Error}", id, result.Error);
    }
    
    return this.ToActionResult(result);
  }
}
