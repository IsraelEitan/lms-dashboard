namespace Lms.Api.Domain.Models;

/// <summary>
/// Represents a course in the LMS.
/// Immutable identifier with mutable content properties for domain operations.
/// </summary>
public sealed class Course
{
    /// <summary>Gets the unique identifier for this course.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gets or sets the short code (e.g., "JS-101"). Must be unique across all courses.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Gets or sets the human-friendly title.</summary>
    public string Title { get; set; } = default!;

    /// <summary>Gets or sets the optional description providing additional course details.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updates the course properties with validated values.
    /// </summary>
    /// <param name="code">The new course code (required, trimmed).</param>
    /// <param name="title">The new course title (required, trimmed).</param>
    /// <param name="description">The optional description (trimmed if provided).</param>
    public void Update(string code, string title, string? description)
    {
        Code = code;
        Title = title;
        Description = description;
    }
}
