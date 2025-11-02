namespace Lms.Api.Contracts.DTOs;

/// <summary>
/// Report showing number of students enrolled per course.
/// </summary>
public readonly record struct CourseEnrollmentReport(
    Guid CourseId,
    string CourseCode,
    string CourseTitle,
    int EnrolledStudents
);

/// <summary>
/// Summary statistics for the dashboard.
/// </summary>
public readonly record struct DashboardStats(
    int TotalStudents,
    int TotalCourses,
    int TotalEnrollments
);

/// <summary>
/// Response after exporting a report to S3.
/// </summary>
public readonly record struct ExportResponse(
    string FileName,
    string S3Url,
    DateTime ExportedAt,
    int RecordCount
);

