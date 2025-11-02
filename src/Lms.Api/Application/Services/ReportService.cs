using System.Text.Json;
using Lms.Api.Application.Interfaces;
using Lms.Api.Common;
using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;

namespace Lms.Api.Application.Services;

/// <summary>
/// Service for generating reports and analytics.
/// Follows Single Responsibility Principle - focused only on report generation logic.
/// </summary>
internal sealed class ReportService : IReportService
{
  private readonly ICourseService _courses;
  private readonly IStudentService _students;
  private readonly IEnrollmentService _enrollments;
  private readonly IS3Service _s3Service;
  private readonly ILogger<ReportService> _logger;

  public ReportService(
    ICourseService courses,
    IStudentService students,
    IEnrollmentService enrollments,
    IS3Service s3Service,
    ILogger<ReportService> logger)
  {
    _courses = courses;
    _students = students;
    _enrollments = enrollments;
    _s3Service = s3Service;
    _logger = logger;
  }

  public async Task<Result<DashboardStats>> GetDashboardStatsAsync(CancellationToken ct = default)
  {
    _logger.LogDebug("Generating dashboard statistics");

    var studentsResult = await _students.GetAllAsync(ct);
    var coursesResult = await _courses.GetAllAsync(ct);
    var enrollmentsResult = await _enrollments.GetAllAsync(ct);

    if (!studentsResult.IsSuccess || !coursesResult.IsSuccess || !enrollmentsResult.IsSuccess)
    {
      _logger.LogWarning("Failed to retrieve data for dashboard statistics");
      return Result<DashboardStats>.Failure(Errors.Common.NotFound("Unable to retrieve statistics"));
    }

    var stats = new DashboardStats(
      TotalStudents: studentsResult.Value!.Count,
      TotalCourses: coursesResult.Value!.Count,
      TotalEnrollments: enrollmentsResult.Value!.Count
    );

    _logger.LogInformation(
      "Dashboard stats generated: {Students} students, {Courses} courses, {Enrollments} enrollments",
      stats.TotalStudents, stats.TotalCourses, stats.TotalEnrollments);

    return Result<DashboardStats>.Success(stats);
  }

  public async Task<Result<IReadOnlyList<CourseEnrollmentReport>>> GetStudentsPerCourseReportAsync(
    CancellationToken ct = default)
  {
    _logger.LogDebug("Generating students-per-course report");

    var coursesResult = await _courses.GetAllAsync(ct);
    var enrollmentsResult = await _enrollments.GetAllAsync(ct);

    if (!coursesResult.IsSuccess || !enrollmentsResult.IsSuccess)
    {
      _logger.LogWarning("Failed to retrieve data for students-per-course report");
      return Result<IReadOnlyList<CourseEnrollmentReport>>.Failure(
        Errors.Common.NotFound("Unable to retrieve report data"));
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
      .ToList()
      .AsReadOnly();

    _logger.LogInformation("Generated students-per-course report with {Count} courses", report.Count);

    return Result<IReadOnlyList<CourseEnrollmentReport>>.Success(report);
  }

  public async Task<Result<ExportResponse>> ExportReportAsync<T>(
    IReadOnlyList<T> reportData,
    string reportName,
    CancellationToken ct = default)
  {
    _logger.LogDebug("Exporting report: {ReportName}", reportName);

    try
    {
      // Serialize to JSON
      var json = JsonSerializer.Serialize(reportData, new JsonSerializerOptions
      {
        WriteIndented = true
      });

      // Generate unique filename with timestamp
      var timestamp = DateTime.UtcNow.ToString(Constants.Reports.TimestampFormat);
      var fileName = $"{Constants.Reports.ExportPrefix}{reportName}_{timestamp}.json";

      // Upload to S3
      var uploadResult = await _s3Service.UploadFileAsync(
        Constants.Reports.BucketName,
        fileName,
        json,
        Constants.Reports.JsonContentType,
        ct);

      if (!uploadResult.IsSuccess)
      {
        _logger.LogError("Failed to upload report to S3: {Error}", uploadResult.Error);
        return Result<ExportResponse>.Failure(uploadResult.Error);
      }

      var response = new ExportResponse(
        FileName: fileName,
        S3Url: uploadResult.Value!,
        ExportedAt: DateTime.UtcNow,
        RecordCount: reportData.Count
      );

      _logger.LogInformation(
        "Exported report to S3: {FileName} ({Count} records)",
        fileName, reportData.Count);

      return Result<ExportResponse>.Success(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error exporting report: {ReportName}", reportName);
      return Result<ExportResponse>.Failure(Errors.Common.Unexpected(ex.Message));
    }
  }
}

