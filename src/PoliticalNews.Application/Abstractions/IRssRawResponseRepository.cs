using PoliticalNews.Domain.Entities;

namespace PoliticalNews.Application.Abstractions;

/// <summary>
/// Archives every RSS fetch exactly as received, regardless of whether parsing succeeded.
/// Write-only from the application's perspective - records are never updated or deleted by
/// business logic (a TTL index handles retention; see <see cref="Options.NewsCrawlerOptions.RawResponseRetention"/>).
/// </summary>
public interface IRssRawResponseRepository
{
    Task InsertAsync(RssRawResponse response, CancellationToken cancellationToken);

    Task<IReadOnlyList<RssRawResponse>> GetRecentAsync(
        string provider, string feedName, int count, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(TimeSpan retention, CancellationToken cancellationToken);
}
