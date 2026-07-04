using System.ComponentModel.DataAnnotations;

namespace Application.Options;

/// <summary>
/// Root configuration section ("NewsCrawler") controlling the crawl scheduler and providers.
/// Validated via <c>ValidateDataAnnotations()</c> at startup - note that this only validates these
/// top-level scalar properties, not the nested <see cref="Providers"/> list items, since .NET's
/// options validation does not recurse into nested complex properties.
/// </summary>
public sealed class NewsCrawlerOptions
{
    public const string SectionName = "NewsCrawler";

    public bool Enabled { get; set; } = true;

    /// <summary>Max number of articles persisted per feed, per run.</summary>
    [Range(1, int.MaxValue)]
    public int BatchSize { get; set; } = 100;

    /// <summary>Name of the distributed lock used to prevent overlapping/concurrent crawl runs.</summary>
    [Required]
    public string LockName { get; set; } = "news-crawler";

    /// <summary>How long a crawl lock is held before it is considered stale and reclaimable.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan LockTtl { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Per-feed HTTP timeout.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "00:10:00")]
    public TimeSpan FeedTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long raw RSS responses (<c>RssRawResponses</c>) are kept - enforced both passively by
    /// a Mongo TTL index and actively by the scheduled job on <see cref="RawResponseCleanupCron"/>.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "365.00:00:00")]
    public TimeSpan RawResponseRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Standard 5-field cron expression for the recurring job that deletes raw responses older
    /// than <see cref="RawResponseRetention"/>. Default: every day at 05:00 IST (falls back to UTC
    /// only if the host's tzdata lacks the "Asia/Kolkata" id - see HangfireRecurringJobRegistrar).
    /// </summary>
    [Required]
    public string RawResponseCleanupCron { get; set; } = "0 5 * * *";

    /// <summary>
    /// Master switch for persisting raw RSS responses to <c>RssRawResponses</c>. A provider only
    /// has its raw responses saved when this is true AND that provider's own
    /// <see cref="RssProviderOptions.SaveRawResponses"/> is also true.
    /// </summary>
    public bool SaveRawResponses { get; set; } = true;

    public List<RssProviderOptions> Providers { get; set; } = [];
}
