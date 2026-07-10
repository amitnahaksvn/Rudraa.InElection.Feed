namespace Application.Crawl.Dtos;

/// <summary>
/// One provider's row in the crawl-report page's breakdown table - configuration (country, cron)
/// merged with its live Hangfire schedule state (next/last execution come from the recurring job
/// itself, not the date range below, so they always reflect "right now" regardless of which
/// window is selected) and its history aggregates *within the selected date range*.
/// <see cref="HasRun"/> is false for a configured, enabled provider that simply hasn't executed
/// yet (a brand-new provider, or one whose cron hasn't ticked within the window) - shown with
/// zeroed counters rather than omitted, so the table is always the complete provider list.
/// </summary>
public sealed record CrawlReportProviderDto(
    string Country,
    string Provider,
    bool HasRun,
    string? Cron,
    string? TimeZone,
    DateTimeOffset? NextExecution,
    DateTimeOffset? LastExecution,
    string? LastJobState,
    string? LastErrorMessage,
    int TotalRuns,
    int SuccessfulRuns,
    int RunsWithErrors,
    int FailedRuns,
    int SkippedRuns,
    double SuccessRatePercent,
    int NewArticles,
    int UpdatedArticles,
    int DuplicateArticles,
    int FailedFeeds);
