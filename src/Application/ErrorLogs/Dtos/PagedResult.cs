namespace Application.ErrorLogs.Dtos;

/// <summary>Generic page envelope - currently only used by <c>GetErrorLogsQuery</c>, kept generic rather than error-log-specific since any future paged list query can reuse it as-is.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, long TotalCount, int Page, int PageSize)
{
    public bool HasMore => (long)Page * PageSize < TotalCount;
}
