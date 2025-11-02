namespace Lms.Api.Domain.Models;

/// <summary>
/// Represents a student in the LMS.
/// Immutable identifier with mutable profile properties for domain operations.
/// </summary>
public sealed class Student
{
    /// <summary>Gets the unique identifier for this student.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gets or sets the student's name.</summary>
    public string Name { get; set; } = default!;

    /// <summary>Gets or sets the student's email address. Should be unique and validated.</summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Updates the student's profile information.
    /// </summary>
    /// <param name="name">The new name (required, trimmed).</param>
    /// <param name="email">The new email address (required, trimmed).</param>
    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
    }
}
