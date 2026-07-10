namespace Application.Crawl.Dtos;

/// <summary>
/// Headline numbers for one pipeline (RSS or API) over a date range - the crawl-report page's
/// stat-tile row. "Messages" is every article a feed/endpoint actually returned (New + Updated +
/// Duplicate); "Saved" is what got persisted (New + Updated); "SkippedDuplicates" is what didn't
/// need to be (already existed) - the three figures the report was built to answer directly.
/// <see cref="SkippedRuns"/> is a separate, run-level concept (a whole run skipped because
/// another run already held that provider's crawl lock) - always 0 today since a lock-skipped run
/// isn't persisted to <c>CrawlHistory</c> at all (see <c>NewsCrawlerOrchestrator</c>/
/// <c>NewsApiCrawlerOrchestrator</c>); kept as its own field so the report stays correct without
/// changes if that ever does get persisted.
/// </summary>
public sealed record CrawlReportSummaryDto(
    int TotalRuns,
    int SuccessfulRuns,
    int RunsWithErrors,
    int FailedRuns,
    int SkippedRuns,
    double SuccessRatePercent,
    int Messages,
    int Saved,
    int NewArticles,
    int UpdatedArticles,
    int SkippedDuplicates,
    int FailedFeeds);
