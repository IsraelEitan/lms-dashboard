using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Contracts.DTOs;

/// <summary>API-facing enrollment view with expanded student and course details.</summary>
public readonly record struct EnrollmentDto(
  Guid Id, 
  Guid StudentId, 
  Guid CourseId, 
  DateTime EnrollmentDate,
  string? StudentName,
  string? StudentEmail,
  string? CourseCode,
  string? CourseTitle
);

/// <summary>Payload to assign a student to a course.</summary>
public sealed class AssignStudentRequest
{
    [Required] public Guid StudentId { get; init; }
    [Required] public Guid CourseId { get; init; }
}
