using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Contracts.DTOs;

/// <summary>API-facing course view.</summary>
public readonly record struct CourseDto(Guid Id, string Code, string Title, string? Description);

/// <summary>Payload for creating/updating a course.</summary>
public sealed class CreateUpdateCourseDto
{
    [Required, MinLength(2)] public string Code { get; init; } = default!;
    [Required, MinLength(2)] public string Title { get; init; } = default!;
    public string? Description { get; init; }
}
