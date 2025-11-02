using Lms.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Api.Infrastructure.Extensions;

/// <summary>
/// Translates domain/application Result into proper HTTP responses
/// with RFC7807 ProblemDetails on failures. Keeps controllers thin.
/// </summary>
internal static class ResultExtensions
{
  /// <summary>Maps a non-generic <see cref="Result"/> to 204 NoContent or ProblemDetails.</summary>
  public static ActionResult ToActionResult(this ControllerBase c, Result result)
      => result.IsSuccess ? c.NoContent() : c.ProblemFromError(result.Error);

  /// <summary>
  /// Maps a <see cref="Result{T}"/> to 200 OK or ProblemDetails (no projection).
  /// Useful when your service already returns a DTO.
  /// </summary>
  public static ActionResult<T> ToActionResult<T>(this ControllerBase c, Result<T> result)
      => result.IsSuccess && result.Value is not null
         ? c.Ok(result.Value)
         : c.ProblemFromError(result.Error);

  /// <summary>
  /// Maps a <see cref="Result{TIn}"/> to 200 OK (projected to <typeparamref name="TOut"/>) or ProblemDetails.
  /// </summary>
  public static ActionResult<TOut> ToActionResult<TIn, TOut>(
      this ControllerBase c, Result<TIn> result, Func<TIn, TOut> map)
      => result.IsSuccess && result.Value is not null
         ? c.Ok(map(result.Value))
         : c.ProblemFromError(result.Error);

  /// <summary>
  /// Maps a <see cref="Result{TIn}"/> to 201 Created (projected) or ProblemDetails.
  /// </summary>
  public static ActionResult<TOut> ToCreatedAt<TIn, TOut>(
      this ControllerBase c,
      string actionName,
      object? routeValues,
      Result<TIn> result,
      Func<TIn, TOut> map)
      => result.IsSuccess && result.Value is not null
         ? c.CreatedAtAction(actionName, routeValues, map(result.Value))
         : c.ProblemFromError(result.Error);

  /// <summary>
  /// Convenience: Created(201) when you don't have a Get action/route to point to.
  /// (Returns the body directly, without Location header.)
  /// </summary>
  public static ActionResult<T> ToCreated<T>(this ControllerBase c, Result<T> result)
      => result.IsSuccess && result.Value is not null
         ? c.StatusCode(StatusCodes.Status201Created, result.Value)
         : c.ProblemFromError(result.Error);

  /// <summary>
  /// Builds RFC7807 ProblemDetails from a domain <see cref="Error"/> and sets StatusCode.
  /// Adds helpful context (traceId, path).
  /// </summary>
  private static ActionResult ProblemFromError(this ControllerBase c, Error err)
  {
    var pd = new ProblemDetails
    {
      Title = err.Code,                      // machine-readable code (e.g., "course.not_found")
      Detail = err.Message,                  // human-readable message
      Status = err.HttpStatus,               // e.g., 404, 409, 422, 500
      Type = $"https://httpstatuses.com/{err.HttpStatus}",
      Instance = c.HttpContext.Request.Path
    };

    // add trace id (useful for correlating logs)
    var traceId = c.HttpContext.TraceIdentifier;
    if (!string.IsNullOrWhiteSpace(traceId))
    {
      pd.Extensions["traceId"] = traceId;
    }

    return new ObjectResult(pd) { StatusCode = err.HttpStatus };
  }
}
