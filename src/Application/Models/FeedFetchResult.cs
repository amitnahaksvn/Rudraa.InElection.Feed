namespace Application.Models;

/// <summary>
/// Outcome of downloading and parsing a single RSS feed via <see cref="Abstractions.IRssProvider"/>.
/// Carries the raw response alongside the normalized articles so the orchestrator can archive it
/// verbatim (<c>RssRawResponse</c>) regardless of whether parsing succeeded.
/// </summary>
public sealed class FeedFetchResult
{
    public required string FeedName { get; init; }

    public required string FeedUrl { get; init; }

    public bool Success { get; init; }

    public string? Error { get; init; }

    /// <summary>Full type name of the exception behind <see cref="Error"/>; null on success.</summary>
    public string? ExceptionType { get; init; }

    public string? StackTrace { get; init; }

    /// <summary>Type + message of the innermost exception; null when there was none.</summary>
    public string? InnerException { get; init; }

    public IReadOnlyList<NormalizedArticle> Articles { get; init; } = [];

    public DateTimeOffset FetchedAt { get; init; }

    /// <summary>Null when the request never completed (e.g. DNS/connection failure).</summary>
    public int? HttpStatusCode { get; init; }

    /// <summary>Null when no response body was ever received.</summary>
    public string? RawXml { get; init; }

    /// <summary>SHA-256 of <see cref="RawXml"/>; null when <see cref="RawXml"/> is null.</summary>
    public string? ContentHash { get; init; }

    public long ProcessingDurationMs { get; init; }
}
