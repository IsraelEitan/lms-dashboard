using System.ComponentModel.DataAnnotations;

namespace Lms.Api.Contracts.DTOs;

/// <summary>API-facing student view.</summary>
public readonly record struct StudentDto(Guid Id, string Name, string Email);

/// <summary>Payload for creating a student.</summary>
public sealed class CreateStudentDto
{
    [Required, MinLength(2)] public string Name { get; init; } = default!;
    [Required, EmailAddress] public string Email { get; init; } = default!;
}

/// <summary>Payload for updating a student.</summary>
public sealed class UpdateStudentDto
{
    [Required, MinLength(2)] public string Name { get; init; } = default!;
    [Required, EmailAddress] public string Email { get; init; } = default!;
}
