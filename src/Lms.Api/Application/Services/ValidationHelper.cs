using Lms.Api.Common.Results;

namespace Lms.Api.Application.Services;

/// <summary>
/// Helper class for common validation operations.
/// </summary>
internal static class ValidationHelper
{
  /// <summary>
  /// Validates that a name is not null or whitespace.
  /// </summary>
  public static Result ValidateName(string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure(Errors.Common.Validation("Name is required"));
    }

    return Result.Success();
  }

  /// <summary>
  /// Validates that an email is not null or whitespace.
  /// </summary>
  public static Result ValidateEmail(string? email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      return Result.Failure(Errors.Student.InvalidEmail(email ?? string.Empty));
    }

    return Result.Success();
  }

  /// <summary>
  /// Validates that a course code is not null or whitespace.
  /// </summary>
  public static Result ValidateCourseCode(string? code)
  {
    if (string.IsNullOrWhiteSpace(code))
    {
      return Result.Failure(Errors.Course.CodeRequired);
    }

    return Result.Success();
  }

  /// <summary>
  /// Validates that a course title is not null or whitespace.
  /// </summary>
  public static Result ValidateCourseTitle(string? title)
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Result.Failure(Errors.Course.TitleRequired);
    }

    return Result.Success();
  }
}

