namespace Application.Crawl.Dtos;

/// <summary>Full payload for the crawl-report page's one selected pipeline (RSS or API) and date range: headline stat tiles, a daily time series for the charts, and a per-provider breakdown table.</summary>
public sealed record CrawlReportDto(
    string Pipeline,
    DateTimeOffset From,
    DateTimeOffset To,
    CrawlReportSummaryDto Summary,
    IReadOnlyList<CrawlReportDailyPointDto> TimeSeries,
    IReadOnlyList<CrawlReportProviderDto> Providers);
