using Lms.Api.Application.Interfaces;
using Lms.Api.Common;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Infrastructure.Attributes;
using Lms.Api.Infrastructure.Extensions;
using Lms.Api.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Api.Presentation.Controllers;

/// <summary>
/// Handles student enrollments in courses.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class EnrollmentsController : ControllerBase
{
  private readonly IEnrollmentService _enrollments;
  private readonly ILogger<EnrollmentsController> _logger;

  /// <summary>DI constructor.</summary>
  public EnrollmentsController(IEnrollmentService enrollments, ILogger<EnrollmentsController> logger)
  {
    _enrollments = enrollments;
    _logger = logger;
  }

  [HttpGet]
  [ProducesResponseType(typeof(PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<PagedResult<EnrollmentDto>>> GetAll([FromQuery] PagingQuery query, CancellationToken ct)
  {
    _logger.LogInformation("[GET] /enrollments - Page: {Page}, PageSize: {PageSize}", query.Page, query.PageSize);
    var result = await _enrollments.QueryAsync(query, ct);
    return this.ToActionResult(result);
  }

  /// <summary>Gets enrollments by course.</summary>
  [HttpGet("by-course/{courseId:guid}")]
  [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetByCourse(Guid courseId, CancellationToken ct)
  {
    var result = await _enrollments.GetByCourseAsync(courseId, ct);
    return this.ToActionResult(result, list => list);
  }

  /// <summary>Gets enrollments by student.</summary>
  [HttpGet("by-student/{studentId:guid}")]
  [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetByStudent(Guid studentId, CancellationToken ct)
  {
    var result = await _enrollments.GetByStudentAsync(studentId, ct);
    return this.ToActionResult(result, list => list);
  }

  /// <summary>Assigns a student to a course.</summary>
  /// <remarks>
  /// This endpoint supports idempotency. Include an `Idempotency-Key` header to ensure
  /// that retrying the request with the same key will not create duplicate enrollments.
  /// </remarks>
  [HttpPost]
  [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<EnrollmentDto>> Assign([FromBody] AssignStudentRequest request, CancellationToken ct)
  {
    _logger.LogInformation("[POST] /enrollments - Enrolling student {StudentId} in course {CourseId}", 
        request.StudentId, request.CourseId);
    
    var result = await _enrollments.AssignAsync(request.StudentId, request.CourseId, ct);
    
    if (result.IsSuccess)
    {
      _logger.LogInformation("Enrollment created successfully: {Id}", result.Value.Id);
    }
    
    return this.ToCreated(result);
  }

  /// <summary>Removes an enrollment.</summary>
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
  {
    _logger.LogInformation("[DELETE] /enrollments/{Id}", id);
    
    var result = await _enrollments.DeleteAsync(id, ct);
    
    if (result.IsSuccess)
    {
      _logger.LogInformation("Enrollment deleted successfully: {Id}", id);
    }
    
    return this.ToActionResult(result);
  }
}
