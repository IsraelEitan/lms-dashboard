using System;

namespace Lms.Api.Infrastructure.Attributes;

/// <summary>
/// Marks an endpoint as requiring idempotency support.
/// The endpoint must include an Idempotency-Key header, and the middleware
/// will cache the response for 24 hours to prevent duplicate processing.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class IdempotencyAttribute : Attribute
{
  /// <summary>
  /// Gets or sets whether the idempotency key is required.
  /// Default is true.
  /// </summary>
  public bool Required { get; set; } = true;
}

