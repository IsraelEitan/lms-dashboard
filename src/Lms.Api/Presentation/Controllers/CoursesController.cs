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
/// Manages course catalog operations (CRUD).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class CoursesController : ControllerBase
{
  private readonly ICourseService _courses;
  private readonly IEnrollmentService _enrollments;
  private readonly ILogger<CoursesController> _logger;

  /// <summary>DI constructor.</summary>
  public CoursesController(ICourseService courses, IEnrollmentService enrollments, ILogger<CoursesController> logger)
  {
    _courses = courses;
    _enrollments = enrollments;
    _logger = logger;
  }

  /// <summary>Gets all courses (paged, sortable, searchable).</summary>
  [HttpGet]
  [ProducesResponseType(typeof(PagedResult<CourseDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<PagedResult<CourseDto>>> GetAll([FromQuery] PagingQuery query, CancellationToken ct)
  {
    _logger.LogInformation("[GET] /courses - Page: {Page}, PageSize: {PageSize}", query.Page, query.PageSize);
    var result = await _courses.QueryAsync(query, ct);
    return this.ToActionResult(result);
  }

  /// <summary>Gets a course by its identifier.</summary>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken ct)
  {
    var result = await _courses.GetByIdAsync(id, ct);
    return this.ToActionResult(result);
  }

  /// <summary>Creates a new course.</summary>
  /// <remarks>
  /// This endpoint supports idempotency. Include an `Idempotency-Key` header to ensure
  /// that retrying the request with the same key will not create duplicate resources.
  /// </remarks>
  [HttpPost]
  [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
  public async Task<ActionResult<CourseDto>> Create([FromBody] CreateUpdateCourseDto dto, CancellationToken ct)
  {
    _logger.LogInformation("[POST] /courses - Creating course: {Code} - {Title}", dto.Code, dto.Title);
    
    var result = await _courses.CreateAsync(dto, ct);
    
    if (!result.IsSuccess)
    {
      _logger.LogWarning("Failed to create course: {Code}, Error: {Error}", dto.Code, result.Error);
      return this.ToActionResult(result);
    }

    _logger.LogInformation("Course created successfully: {Id}, Code: {Code}", result.Value.Id, result.Value.Code);
    
    var routeValues = new { id = result.Value.Id };
    return CreatedAtAction(nameof(GetById), routeValues, result.Value);
  }

  /// <summary>Updates an existing course.</summary>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult> Update(Guid id, [FromBody] CreateUpdateCourseDto dto, CancellationToken ct)
  {
    var result = await _courses.UpdateAsync(id, dto, ct);
    return this.ToActionResult(result);
  }

  /// <summary>Deletes a course and all its enrollments.</summary>
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
  {
    _logger.LogInformation("[DELETE] /courses/{Id} - Deleting course and cascading enrollments", id);
    
    // First delete all enrollments for this course (cascade delete)
    await _enrollments.DeleteByCourseAsync(id, ct);
    
    // Then delete the course
    var result = await _courses.DeleteAsync(id, ct);
    
    if (result.IsSuccess)
    {
      _logger.LogInformation("Course deleted successfully: {Id}", id);
    }
    
    return this.ToActionResult(result);
  }
}
