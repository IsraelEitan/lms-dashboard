using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;

namespace Lms.Api.Application.Interfaces;

/// <summary>
/// Service for generating reports and analytics.
/// </summary>
public interface IReportService
{
  /// <summary>
  /// Gets dashboard statistics (totals).
  /// </summary>
  Task<Result<DashboardStats>> GetDashboardStatsAsync(CancellationToken ct = default);

  /// <summary>
  /// Generates a report of student enrollments per course.
  /// </summary>
  Task<Result<IReadOnlyList<CourseEnrollmentReport>>> GetStudentsPerCourseReportAsync(CancellationToken ct = default);

  /// <summary>
  /// Exports a report to S3 storage.
  /// </summary>
  Task<Result<ExportResponse>> ExportReportAsync<T>(
    IReadOnlyList<T> reportData,
    string reportName,
    CancellationToken ct = default);
}

