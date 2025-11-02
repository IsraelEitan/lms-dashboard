namespace Lms.Api.Common.Results;

/// <summary>
/// Describes a typed error with code, human-readable message, and recommended HTTP status.
/// </summary>
public readonly struct Error
{
    /// <summary>A sentinel empty error for success cases.</summary>
    public static readonly Error None = new("", "");

    /// <summary>Machine-readable error code (e.g., "course.not_found").</summary>
    public string Code { get; }

    /// <summary>Human-readable message intended for logs/clients.</summary>
    public string Message { get; }

    /// <summary>Recommended HTTP status code to map this error to.</summary>
    public int HttpStatus { get; }

    /// <summary>Creates a new <see cref="Error"/> instance.</summary>
    public Error(string code, string message, int httpStatus = 400)
    {
        Code = code; Message = message; HttpStatus = httpStatus;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Code}: {Message}";
}

/// <summary>Centralized error catalog used by the domain/application layers.</summary>
public static class Errors
{
    /// <summary>Common/shared errors.</summary>
    public static class Common
    {
        public static Error Unexpected(string? detail = null) =>
            new("common.unexpected", detail ?? "Unexpected error", 500);

        public static Error Validation(string message) =>
            new("common.validation", message, 422);
    }

    /// <summary>Course-related errors.</summary>
    public static class Course
    {
        public static Error NotFound(Guid id) =>
            new("course.not_found", $"Course '{id}' was not found", 404);

        public static Error CodeRequired =>
            new("course.code_required", "Course code is required", 422);

        public static Error TitleRequired =>
            new("course.title_required", "Course title is required", 422);

        public static Error DuplicateCode(string code) =>
            new("course.duplicate_code", $"Course code '{code}' already exists", 409);
    }

    /// <summary>Student-related errors.</summary>
    public static class Student
    {
        public static Error NotFound(Guid id) =>
            new("student.not_found", $"Student '{id}' was not found", 404);

        public static Error InvalidEmail(string email) =>
            new("student.invalid_email", $"Email '{email}' is invalid", 422);
    }

    /// <summary>Enrollment-related errors.</summary>
    public static class Enrollment
    {
        public static Error StudentMissing(Guid id) =>
            new("enrollment.student_missing", $"Student '{id}' not found", 404);

        public static Error CourseMissing(Guid id) =>
            new("enrollment.course_missing", $"Course '{id}' not found", 404);

        public static Error AlreadyEnrolled(Guid studentId, Guid courseId) =>
            new("enrollment.duplicate", $"Student '{studentId}' already enrolled to course '{courseId}'", 409);

        public static Error NotFound(Guid id) =>
            new("enrollment.not_found", $"Enrollment '{id}' was not found", 404);
    }
}
