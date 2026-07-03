namespace Application.Models;

/// <summary>
/// Outcome of calling one endpoint of one news-API provider via <see cref="Abstractions.INewsApiProvider"/>.
/// The <see cref="FeedFetchResult"/> counterpart for JSON REST APIs - one result per endpoint per
/// run, so one bad endpoint (rate-limited, down, schema changed) never aborts a provider's other
/// endpoints.
/// </summary>
public sealed class ApiFetchResult
{
    public required string EndpointName { get; init; }

    public required string EndpointUrl { get; init; }

    public bool Success { get; init; }

    public string? Error { get; init; }

    public IReadOnlyList<NormalizedArticle> Articles { get; init; } = [];

    public DateTimeOffset FetchedAt { get; init; }

    /// <summary>Null when the request never completed (e.g. DNS/connection failure).</summary>
    public int? HttpStatusCode { get; init; }

    public long ProcessingDurationMs { get; init; }
}
