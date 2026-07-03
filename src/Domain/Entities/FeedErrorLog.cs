namespace Domain.Entities;

/// <summary>
/// A single exception raised while ingesting a <see cref="FeedSource"/>-driven feed, kept
/// separately from <see cref="CrawlHistory.Error"/> (which only holds the latest error's message)
/// so every failure - not just the most recent - is retained for a given feed, with enough detail
/// (full stack trace, a running retry count) to diagnose a feed that's been silently failing.
/// </summary>
public sealed class FeedErrorLog
{
    public string Id { get; set; } = string.Empty;

    public string FeedSourceId { get; set; } = string.Empty;

    public DateTimeOffset OccurredOn { get; set; }

    public string Exception { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public int RetryCount { get; set; }

    public bool Resolved { get; set; }
}
