using System.ComponentModel.DataAnnotations;

namespace Application.Options;

/// <summary>
/// Root configuration section ("NewsApiCrawler") controlling the JSON news-API scheduler and
/// providers - the API-fetching counterpart to <see cref="NewsCrawlerOptions"/> (RSS). Kept as a
/// separate section/orchestrator/lock namespace rather than folded into <see cref="NewsCrawlerOptions"/>
/// because these providers are polled REST APIs with per-request auth and rate limits, not RSS
/// feeds - a fundamentally different fetch shape, even though the resulting articles land in the
/// same <c>NewsArticles</c> collection via the same dedup pipeline. Validated via
/// <c>ValidateDataAnnotations()</c> at startup - only these top-level scalar properties, not the
/// nested <see cref="Providers"/> list items (see <see cref="NewsCrawlerOptions"/>'s own note).
/// </summary>
public sealed class NewsApiCrawlerOptions
{
    public const string SectionName = "NewsApiCrawler";

    public bool Enabled { get; set; } = true;

    /// <summary>Max number of articles persisted per provider, per run.</summary>
    [Range(1, int.MaxValue)]
    public int BatchSize { get; set; } = 100;

    /// <summary>Name of the distributed lock used to prevent overlapping/concurrent API crawl runs.</summary>
    [Required]
    public string LockName { get; set; } = "news-api-crawler";

    /// <summary>How long an API crawl lock is held before it is considered stale and reclaimable.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan LockTtl { get; set; } = TimeSpan.FromMinutes(15);

    public List<NewsApiProviderOptions> Providers { get; set; } = [];
}
