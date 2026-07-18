namespace n8neiritech.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResult<T> Create(IEnumerable<T> items, int page, int pageSize, int totalCount) =>
        new()
        {
            Items = items.ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
}
