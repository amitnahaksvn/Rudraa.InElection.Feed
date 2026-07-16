using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// A record of a single crawler execution, spanning every provider/feed processed in that run.
/// </summary>
public sealed class CrawlHistory
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Which fetch pipeline produced this run - lets a report/history query tell RSS, API, and Social runs apart.</summary>
    public CrawlPipeline Pipeline { get; set; }

    /// <summary>
    /// Distinct provider names actually processed by this run. In the common case (a provider's
    /// own scheduled Hangfire job) this has exactly one entry; a manual "trigger everything"
    /// call (no provider filter) can span many, since <c>RunLockedAsync</c> loops every locked
    /// provider into one record rather than one record per provider in that case.
    /// </summary>
    public List<string> Providers { get; set; } = [];

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public TimeSpan? Duration { get; set; }

    public int FeedCount { get; set; }

    public int NewArticles { get; set; }

    public List<string> FailedFeeds { get; set; } = [];

    public CrawlStatus Status { get; set; } = CrawlStatus.Running;

    public string? Error { get; set; }
}
