namespace Infrastructure.Scheduling;

/// <summary>
/// Single source of truth for the recurring-job-id naming convention, shared between whoever
/// registers a provider's job (<c>Worker</c>) and whoever triggers it on demand
/// (<see cref="HangfireCrawlJobTrigger"/>) so the two can never drift apart.
/// </summary>
public static class HangfireJobIds
{
    public static string NewsCrawl(string providerName) => $"news-crawl-{providerName}";

    public const string RawResponseCleanup = "cleanup-raw-responses";

    /// <summary>Job id for a <see cref="Domain.Entities.FeedSource"/>-driven feed, keyed by its own SourceCode (e.g. "dynamic-feed-PIB").</summary>
    public static string DynamicFeed(string sourceCode) => $"dynamic-feed-{sourceCode}";

    /// <summary>Job id for a JSON news-API provider (e.g. "news-api-NewsApiOrg").</summary>
    public static string NewsApi(string providerName) => $"news-api-{providerName}";
}
