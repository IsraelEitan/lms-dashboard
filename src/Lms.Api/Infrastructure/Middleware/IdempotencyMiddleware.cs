using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lms.Api.Common;
using Lms.Api.Infrastructure.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lms.Api.Infrastructure.Middleware;

/// <summary>
/// Middleware that implements idempotency for POST requests by caching responses
/// based on the Idempotency-Key header. This prevents duplicate processing of the same request.
/// </summary>
/// <remarks>
/// <para>
/// When a POST request includes an Idempotency-Key header on an endpoint marked with
/// <see cref="IdempotencyAttribute"/>, the middleware:
/// </para>
/// <list type="bullet">
///   <item>Checks if a cached response exists for that key</item>
///   <item>Returns the cached response if found (same status code, headers, and body)</item>
///   <item>Otherwise, executes the request and caches the response for 24 hours</item>
/// </list>
/// <para>
/// This ensures that retrying a request with the same idempotency key will not create
/// duplicate resources or side effects, making the API more resilient to network issues
/// and client retries.
/// </para>
/// </remarks>
public sealed class IdempotencyMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IMemoryCache _cache;
  private readonly ILogger<IdempotencyMiddleware> _logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="IdempotencyMiddleware"/> class.
  /// </summary>
  /// <param name="next">The next middleware in the pipeline.</param>
  /// <param name="cache">The memory cache for storing idempotency responses.</param>
  /// <param name="logger">The logger for diagnostic information.</param>
  public IdempotencyMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<IdempotencyMiddleware> logger)
  {
    _next = next ?? throw new ArgumentNullException(nameof(next));
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Invokes the middleware to process the HTTP request.
  /// </summary>
  /// <param name="context">The HTTP context for the current request.</param>
  public async Task InvokeAsync(HttpContext context)
  {
    // Only process POST requests
    if (!HttpMethods.IsPost(context.Request.Method))
    {
      await _next(context);
      return;
    }

    // Check if endpoint has IdempotencyAttribute
    var endpoint = context.GetEndpoint();
    if (endpoint == null)
    {
      await _next(context);
      return;
    }

    // Try to get IdempotencyAttribute from endpoint metadata first, then from method attributes
    var idempotencyAttr = endpoint.Metadata.GetMetadata<IdempotencyAttribute>();
    
    if (idempotencyAttr == null)
    {
      // Fallback to method attributes
      var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
      idempotencyAttr = controllerActionDescriptor?.MethodInfo.GetCustomAttributes(typeof(IdempotencyAttribute), false)
        .Cast<IdempotencyAttribute>()
        .FirstOrDefault();
    }

    if (idempotencyAttr == null)
    {
      // No idempotency required
      await _next(context);
      return;
    }

    // Extract idempotency key from header
    if (!context.Request.Headers.TryGetValue(Constants.Idempotency.HeaderName, out var idempotencyKey))
    {
      if (idempotencyAttr.Required)
      {
        _logger.LogWarning("POST request missing required Idempotency-Key header");
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
          type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          title = "Bad Request",
          status = 400,
          detail = Constants.Idempotency.MissingKeyError
        });
        return;
      }
      else
      {
        // Idempotency is optional, continue without it
        await _next(context);
        return;
      }
    }

    var key = idempotencyKey.ToString();

    // Validate key length
    if (string.IsNullOrWhiteSpace(key) || key.Length > Constants.Idempotency.MaxKeyLength)
    {
      _logger.LogWarning("Invalid Idempotency-Key: length={Length}", key?.Length ?? 0);
      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      context.Response.ContentType = "application/problem+json";
      await context.Response.WriteAsJsonAsync(new
      {
        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        title = "Bad Request",
        status = 400,
        detail = Constants.Idempotency.InvalidKeyError
      });
      return;
    }

    var cacheKey = $"{Constants.Idempotency.CacheKeyPrefix}{key}";

    // Check if we have a cached response
    if (_cache.TryGetValue<CachedResponse>(cacheKey, out var cachedResponse) && cachedResponse != null)
    {
      _logger.LogInformation("Returning cached response for idempotency key: {Key}", key);
      
      // Restore the cached response
      context.Response.StatusCode = cachedResponse.StatusCode;
      context.Response.ContentType = cachedResponse.ContentType;
      
      foreach (var header in cachedResponse.Headers)
      {
        context.Response.Headers[header.Key] = header.Value;
      }
      
      if (cachedResponse.Body != null && cachedResponse.Body.Length > 0)
      {
        await context.Response.Body.WriteAsync(cachedResponse.Body, 0, cachedResponse.Body.Length);
      }
      
      return;
    }

    // Capture the response
    var originalBodyStream = context.Response.Body;
    using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    try
    {
      // Execute the request
      await _next(context);

      // Only cache successful responses (2xx status codes)
      if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
      {
        // Read the response body
        responseBody.Seek(0, SeekOrigin.Begin);
        var bodyBytes = responseBody.ToArray();

        // Cache the response
        var cached = new CachedResponse
        {
          StatusCode = context.Response.StatusCode,
          ContentType = context.Response.ContentType ?? "application/json",
          Body = bodyBytes,
          Headers = context.Response.Headers
            .Where(h => !h.Key.StartsWith("Transfer-", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString())
        };

        _cache.Set(cacheKey, cached, Constants.Idempotency.CacheDuration);
        _logger.LogInformation("Cached response for idempotency key: {Key}, TTL: {TTL}", key, Constants.Idempotency.CacheDuration);
      }

      // Copy the response back to the original stream
      responseBody.Seek(0, SeekOrigin.Begin);
      await responseBody.CopyToAsync(originalBodyStream);
    }
    finally
    {
      context.Response.Body = originalBodyStream;
    }
  }

  /// <summary>
  /// Represents a cached HTTP response for idempotency.
  /// </summary>
  private sealed class CachedResponse
  {
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = default!;
    public byte[] Body { get; set; } = default!;
    public Dictionary<string, string> Headers { get; set; } = new();
  }
}

