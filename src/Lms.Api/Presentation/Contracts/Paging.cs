namespace Lms.Api.Presentation.Contracts;

public sealed class PagingQuery
{
  public int Page { get; init; } = 1;       // ?page=1
  public int PageSize { get; init; } = 20;  // ?pageSize=20  (we'll clamp)
  public string? Sort { get; init; }        // ?sort=code,-title  (minus = desc)
  public string? Search { get; init; }      // ?search=basics
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalPages,
    int TotalCount,
    bool HasPreviousPage,
    bool HasNextPage
);
