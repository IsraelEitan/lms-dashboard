using Lms.Api.Presentation.Contracts;

namespace Lms.Api.Application.Services;

/// <summary>
/// Helper class for common query operations like sorting, filtering, and pagination.
/// </summary>
internal static class QueryHelper<T>
{
  /// <summary>
  /// Applies sorting to a collection based on the query sort parameter.
  /// </summary>
  /// <param name="source">The source collection to sort.</param>
  /// <param name="query">The paging query containing sort instructions.</param>
  /// <param name="sortKeySelector">Function to select the sort key based on the sort field name.</param>
  /// <returns>Sorted collection or original if no sort specified.</returns>
  public static IEnumerable<T> ApplySort(
    IEnumerable<T> source,
    PagingQuery query,
    Func<T, string, object?> sortKeySelector)
  {
    if (string.IsNullOrWhiteSpace(query.Sort))
    {
      return source;
    }

    var keys = query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    
    IOrderedEnumerable<T> ordered = keys[0].StartsWith('-')
      ? source.OrderByDescending(item => sortKeySelector(item, keys[0][1..]))
      : source.OrderBy(item => sortKeySelector(item, keys[0]));

    foreach (var key in keys.Skip(1))
    {
      ordered = key.StartsWith('-')
        ? ordered.ThenByDescending(item => sortKeySelector(item, key[1..]))
        : ordered.ThenBy(item => sortKeySelector(item, key));
    }

    return ordered;
  }

  /// <summary>
  /// Applies search filter to a collection.
  /// </summary>
  /// <param name="source">The source collection to filter.</param>
  /// <param name="query">The paging query containing search term.</param>
  /// <param name="searchPredicate">Function to determine if an item matches the search.</param>
  /// <returns>Filtered collection or original if no search specified.</returns>
  public static IEnumerable<T> ApplySearch(
    IEnumerable<T> source,
    PagingQuery query,
    Func<T, string, bool> searchPredicate)
  {
    if (string.IsNullOrWhiteSpace(query.Search))
    {
      return source;
    }

    return source.Where(item => searchPredicate(item, query.Search));
  }

  /// <summary>
  /// Applies pagination to a collection and returns a paged result.
  /// </summary>
  /// <typeparam name="TDto">The DTO type for the result items.</typeparam>
  /// <param name="source">The source collection to paginate.</param>
  /// <param name="query">The paging query containing page number and size.</param>
  /// <param name="mapper">Function to map domain entities to DTOs.</param>
  /// <returns>A paged result containing the items and pagination metadata.</returns>
  public static PagedResult<TDto> ApplyPagination<TDto>(
    IEnumerable<T> source,
    PagingQuery query,
    Func<T, TDto> mapper)
  {
    var list = source.ToList();
    var page = Math.Max(1, query.Page);
    var size = Math.Clamp(query.PageSize, 1, 100);
    var total = list.Count;
    var totalPages = (int)Math.Ceiling((double)total / size);
    
    var items = list
      .Skip((page - 1) * size)
      .Take(size)
      .Select(mapper)
      .ToList();

    return new PagedResult<TDto>(
      items,
      page,
      size,
      totalPages,
      total,
      page > 1,
      page < totalPages
    );
  }
}

