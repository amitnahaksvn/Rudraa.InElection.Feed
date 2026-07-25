namespace Application.Crawl.Dtos;

/// <summary>
/// One (day, provider) bucket's real article count - the article-volume report's chart data,
/// broken down by provider per day rather than collapsed into a single daily total. Sparse: a
/// (day, provider) combination with zero articles simply has no row, rather than an explicit
/// zero-count entry - the frontend zero-fills against its own known date range when charting.
/// </summary>
public sealed record ArticleVolumeProviderDailyPointDto(DateOnly Date, string Provider, int Count);
