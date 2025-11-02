namespace Lms.Api.Domain.Models;

/// <summary>
/// Represents a student's enrollment in a specific course.
/// Immutable once created as enrollments represent historical facts.
/// </summary>
public sealed class Enrollment
{
    /// <summary>Gets the unique identifier for this enrollment.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gets the student identifier.</summary>
    public Guid StudentId { get; init; }

    /// <summary>Gets the course identifier.</summary>
    public Guid CourseId { get; init; }

    /// <summary>Gets the UTC timestamp when the enrollment was created.</summary>
    public DateTime EnrolledAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new enrollment for a student in a course.
    /// </summary>
    /// <param name="studentId">The ID of the student being enrolled.</param>
    /// <param name="courseId">The ID of the course to enroll in.</param>
    /// <returns>A new enrollment instance.</returns>
    public static Enrollment Create(Guid studentId, Guid courseId)
    {
        return new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId
        };
    }
}
