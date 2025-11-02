using Lms.Api.Application.Interfaces;
using Lms.Api.Common;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Api.Presentation.Controllers;

/// <summary>
/// Reports and analytics endpoints.
/// Follows Single Responsibility Principle - only handles HTTP concerns, delegates business logic to services.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class ReportsController : ControllerBase
{
  private readonly IReportService _reportService;
  private readonly IS3Service _s3Service;
  private readonly ILogger<ReportsController> _logger;

  public ReportsController(
    IReportService reportService,
    IS3Service s3Service,
    ILogger<ReportsController> logger)
  {
    _reportService = reportService;
    _s3Service = s3Service;
    _logger = logger;
  }

  /// <summary>
  /// Get dashboard statistics (total students, courses, enrollments).
  /// </summary>
  [HttpGet("dashboard-stats")]
  [ProducesResponseType(typeof(DashboardStats), StatusCodes.Status200OK)]
  public async Task<ActionResult<DashboardStats>> GetDashboardStats(CancellationToken ct)
  {
    _logger.LogInformation("[GET] /reports/dashboard-stats - Fetching dashboard statistics");

    var result = await _reportService.GetDashboardStatsAsync(ct);
    return this.ToActionResult(result);
  }

  /// <summary>
  /// Get course enrollment report (students per course).
  /// </summary>
  /// <remarks>
  /// Returns a list of courses with their enrollment counts.
  /// Useful for analyzing course popularity and capacity planning.
  /// </remarks>
  [HttpGet("students-per-course")]
  [ProducesResponseType(typeof(IReadOnlyList<CourseEnrollmentReport>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<CourseEnrollmentReport>>> GetStudentsPerCourse(CancellationToken ct)
  {
    _logger.LogInformation("[GET] /reports/students-per-course - Generating course enrollment report");

    var result = await _reportService.GetStudentsPerCourseReportAsync(ct);
    return this.ToActionResult(result, list => list);
  }

  /// <summary>
  /// Export students-per-course report to S3 as JSON.
  /// </summary>
  /// <remarks>
  /// This demonstrates AWS S3 integration (mocked).
  /// In production, this would save to actual S3 bucket.
  /// </remarks>
  [HttpPost("export/students-per-course")]
  [ProducesResponseType(typeof(ExportResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ExportResponse>> ExportStudentsPerCourse(CancellationToken ct)
  {
    _logger.LogInformation("[POST] /reports/export/students-per-course - Exporting report");

    // Get report data
    var reportResult = await _reportService.GetStudentsPerCourseReportAsync(ct);
    if (!reportResult.IsSuccess)
    {
      _logger.LogWarning("Failed to generate report for export");
      return BadRequest(reportResult.Error);
    }

    // Export to S3
    var exportResult = await _reportService.ExportReportAsync(
      reportResult.Value!,
      "students-per-course",
      ct);

    return this.ToActionResult(exportResult);
  }

  /// <summary>
  /// List all exported reports from S3.
  /// </summary>
  [HttpGet("exports")]
  [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<string>>> ListExports(CancellationToken ct)
  {
    _logger.LogInformation("[GET] /reports/exports - Listing exported reports");

    var result = await _s3Service.ListFilesAsync(
      Constants.Reports.BucketName,
      Constants.Reports.ExportPrefix,
      ct);

    return this.ToActionResult(result, list => list);
  }

  /// <summary>
  /// Download an exported report from S3.
  /// </summary>
  [HttpGet("exports/{*fileName}")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<string>> DownloadExport(string fileName, CancellationToken ct)
  {
    _logger.LogInformation("[GET] /reports/exports/{FileName} - Downloading report", fileName);

    var result = await _s3Service.DownloadFileAsync(Constants.Reports.BucketName, fileName, ct);
    return this.ToActionResult(result);
  }
}
