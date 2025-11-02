using Lms.Api.Application.Interfaces;
using Lms.Api.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Lms.Api.Presentation.Controllers;

/// <summary>
/// Reports and analytics endpoints.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class ReportsController : ControllerBase
{
    private readonly ICourseService _courses;
    private readonly IStudentService _students;
    private readonly IEnrollmentService _enrollments;
    private readonly IS3Service _s3Service;
    private readonly ILogger<ReportsController> _logger;

    private const string ReportBucket = "lms-reports";

    public ReportsController(
        ICourseService courses,
        IStudentService students,
        IEnrollmentService enrollments,
        IS3Service s3Service,
        ILogger<ReportsController> logger)
    {
        _courses = courses;
        _students = students;
        _enrollments = enrollments;
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
        
        var students = await _students.GetAllAsync(ct);
        var courses = await _courses.GetAllAsync(ct);
        var enrollments = await _enrollments.GetAllAsync(ct);

        var stats = new DashboardStats(
            TotalStudents: students.Value!.Count,
            TotalCourses: courses.Value!.Count,
            TotalEnrollments: enrollments.Value!.Count
        );

        _logger.LogInformation("Dashboard stats: {Students} students, {Courses} courses, {Enrollments} enrollments", 
            stats.TotalStudents, stats.TotalCourses, stats.TotalEnrollments);

        return Ok(stats);
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
        
        var coursesResult = await _courses.GetAllAsync(ct);
        var enrollmentsResult = await _enrollments.GetAllAsync(ct);

        if (!coursesResult.IsSuccess || !enrollmentsResult.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve data for students-per-course report");
            return Problem("Failed to retrieve data for report");
        }

        var courses = coursesResult.Value!;
        var enrollments = enrollmentsResult.Value!;

        // Group enrollments by course
        var enrollmentCounts = enrollments
            .GroupBy(e => e.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Build report
        var report = courses
            .Select(course => new CourseEnrollmentReport(
                CourseId: course.Id,
                CourseCode: course.Code,
                CourseTitle: course.Title,
                EnrolledStudents: enrollmentCounts.GetValueOrDefault(course.Id, 0)
            ))
            .OrderByDescending(r => r.EnrolledStudents)
            .ThenBy(r => r.CourseCode)
            .ToList();

        _logger.LogInformation("Generated students-per-course report with {Count} courses", report.Count);

        return Ok(report);
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
    public async Task<ActionResult<ExportResponse>> ExportStudentsPerCourse(CancellationToken ct)
    {
        // Get the report data
        var reportResult = await GetStudentsPerCourse(ct);
        if (reportResult.Result is not OkObjectResult okResult)
        {
            return Problem("Failed to generate report");
        }

        var report = (IReadOnlyList<CourseEnrollmentReport>)okResult.Value!;

        // Serialize to JSON
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        // Generate unique filename
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        var fileName = $"reports/students-per-course_{timestamp}.json";

        // Upload to S3 (mocked)
        var s3Url = await _s3Service.UploadFileAsync(
            bucketName: ReportBucket,
            key: fileName,
            content: json,
            contentType: "application/json"
        );

        var response = new ExportResponse(
            FileName: fileName,
            S3Url: s3Url,
            ExportedAt: DateTime.UtcNow,
            RecordCount: report.Count
        );

        _logger.LogInformation(
            "✅ Exported students-per-course report to S3: {FileName} ({Count} records)",
            fileName, report.Count);

        return Ok(response);
    }

    /// <summary>
    /// List all exported reports from S3.
    /// </summary>
    [HttpGet("exports")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> ListExports()
    {
        var files = await _s3Service.ListFilesAsync(ReportBucket, prefix: "reports/");
        
        _logger.LogInformation("Listed {Count} exported reports from S3", files.Count);
        
        return Ok(files);
    }

    /// <summary>
    /// Download an exported report from S3.
    /// </summary>
    [HttpGet("exports/{*fileName}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> DownloadExport(string fileName)
    {
        var content = await _s3Service.DownloadFileAsync(ReportBucket, fileName);
        
        if (content == null)
        {
            return NotFound(new { error = $"Report '{fileName}' not found" });
        }

        _logger.LogInformation("Downloaded report from S3: {FileName}", fileName);
        
        return Ok(content);
    }
}

