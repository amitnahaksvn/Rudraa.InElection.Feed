namespace Application.Crawl.Dtos;

/// <summary>
/// Full payload for the article-volume report page's one selected pipeline (RSS or API) and date
/// range - same "look and concept" as the crawl-report page (date range, provider filter, headline
/// tile, daily chart) but scoped purely to how many articles actually came in per provider, sourced
/// straight from <see cref="Domain.Entities.ArticleFingerprint"/> rather than
/// <see cref="Domain.Entities.CrawlHistory"/>/Hangfire schedule state. <see cref="ProviderTimeSeries"/>
/// is per-(day, provider), not collapsed into one daily total - the chart breaks volume down by
/// provider per day rather than showing a single combined line.
/// </summary>
public sealed record ArticleVolumeReportDto(
    string Pipeline,
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalArticles,
    IReadOnlyList<ArticleVolumeProviderDailyPointDto> ProviderTimeSeries,
    IReadOnlyList<ArticleVolumeProviderDto> Providers);
