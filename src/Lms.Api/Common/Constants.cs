using System;

namespace Lms.Api.Common;

/// <summary>
/// One-stop, type-safe home for all literals: routes, cache keys/policies, swagger text, and shared messages.
/// </summary>
public static class Constants
{
  public static class Routes
  {
    public static class Courses
    {
      public const string Base = "api/courses";
      public const string ById = $"{Base}/{{id:guid}}";
    }

    public static class Students
    {
      public const string Base = "api/students";
      public const string ById = $"{Base}/{{id:guid}}";
    }

    public static class Enrollments
    {
      public const string Base = "api/enrollments";
      public const string ByCourse = $"{Base}/by-course/{{courseId:guid}}";
      public const string ByStudent = $"{Base}/by-student/{{studentId:guid}}";
    }
  }

  public static class CachePolicies
  {
    public const string ShortList = "ShortList";
  }

  public static class CacheDurations
  {
    public static readonly TimeSpan ShortList = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Item = TimeSpan.FromSeconds(60);
  }

  public static class CacheKeys
  {
    public static class Courses
    {
      public const string All = "courses:all";
      public static string ById(Guid id) => $"courses:id:{id:N}";
    }

    public static class Students
    {
      public const string All = "students:all";
      public static string ById(Guid id) => $"students:id:{id:N}";
    }
  }

  public static class Swagger
  {
    public const string Title = "LMS Dashboard API";
    public const string Version = "v1";
    public const string Description = "Learning Management Dashboard — clean architecture, async, Result pattern.";
  }

  public static class ErrorMessages
  {
    public const string Unexpected = "Unexpected error";
    public const string EmptyPayload = "Response payload was empty.";
    public const string Timeout = "Upstream timeout.";
  }

  public static class Idempotency
  {
    public const string HeaderName = "Idempotency-Key";
    public const string CacheKeyPrefix = "idempotency:";
    public static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    public const int MaxKeyLength = 255;
    public const string MissingKeyError = "Idempotency-Key header is required for this operation";
    public const string InvalidKeyError = "Idempotency-Key must be between 1 and 255 characters";
  }

  public static class Reports
  {
    public const string BucketName = "lms-reports";
    public const string ExportPrefix = "reports/";
    public const string JsonContentType = "application/json";
    public const string TimestampFormat = "yyyy-MM-dd_HHmmss";
  }
}
