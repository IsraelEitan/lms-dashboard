namespace Lms.Api.Application.Services;

internal static class CacheKeys
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
