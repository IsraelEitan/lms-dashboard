using Lms.Api.Common;
using Lms.Api.Common.Results;

namespace Lms.Api.Infrastructure.Middleware;

/// <summary>
/// Global exception boundary that converts unhandled exceptions to RFC7807 ProblemDetails
/// and avoids leaking stack traces in production.
/// </summary>
internal sealed class ExceptionHandlingMiddleware : IMiddleware
{
  private readonly ILogger<ExceptionHandlingMiddleware> _log;
  private readonly IHostEnvironment _env;

  public ExceptionHandlingMiddleware(
      ILogger<ExceptionHandlingMiddleware> log,
      IHostEnvironment env)
  {
    _log = log;
    _env = env;
  }

  /// <inheritdoc />
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    try
    {
      await next(context);
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
      // Client aborted request
      context.Response.StatusCode = 499;
    }
    catch (Exception ex)
    {
      _log.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
      var detail = _env.IsDevelopment() ? ex.ToString() : Constants.ErrorMessages.Unexpected;

      context.Response.StatusCode = StatusCodes.Status500InternalServerError;

      await Results.Problem(
          title: Errors.Common.Unexpected().Code,
          detail: detail,
          statusCode: StatusCodes.Status500InternalServerError
      ).ExecuteAsync(context);
    }
  }
}
