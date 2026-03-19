namespace TaskManager.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; }
    public int Total { get; }
    public int Page { get; }
    public int PageSize { get; }

    public PagedResult(IReadOnlyList<T> data, int total, int page, int pageSize)
    {
        Data = data;
        Total = total;
        Page = page;
        PageSize = pageSize;
    }
}
