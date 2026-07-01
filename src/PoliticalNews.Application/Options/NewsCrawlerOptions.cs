namespace PoliticalNews.Application.Options;

/// <summary>
/// Root configuration section ("NewsCrawler") controlling the crawl scheduler and providers.
/// </summary>
public sealed class NewsCrawlerOptions
{
    public const string SectionName = "NewsCrawler";

    public bool Enabled { get; set; } = true;

    /// <summary>Standard 5-field cron expression, e.g. "*/5 * * * *" for every 5 minutes.</summary>
    public string Cron { get; set; } = "*/5 * * * *";

    /// <summary>Max number of articles persisted per feed, per run.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Name of the distributed lock used to prevent overlapping/concurrent crawl runs.</summary>
    public string LockName { get; set; } = "news-crawler";

    /// <summary>How long a crawl lock is held before it is considered stale and reclaimable.</summary>
    public TimeSpan LockTtl { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Per-feed HTTP timeout.</summary>
    public TimeSpan FeedTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>How long raw RSS responses (<c>RssRawResponses</c>) are kept before a TTL index expires them.</summary>
    public TimeSpan RawResponseRetention { get; set; } = TimeSpan.FromDays(30);

    public List<RssProviderOptions> Providers { get; set; } = [];
}
