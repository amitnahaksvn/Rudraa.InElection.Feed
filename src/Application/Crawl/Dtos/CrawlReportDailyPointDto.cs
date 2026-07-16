namespace Application.Crawl.Dtos;

/// <summary>One UTC calendar day's totals within a <see cref="CrawlReportDto"/> - backs the report's time-series charts (runs-by-outcome, articles-by-outcome).</summary>
public sealed record CrawlReportDailyPointDto(
    DateOnly Date,
    int TotalRuns,
    int SuccessfulRuns,
    int RunsWithErrors,
    int FailedRuns,
    int SkippedRuns,
    int NewArticles,
    int FailedFeeds);
