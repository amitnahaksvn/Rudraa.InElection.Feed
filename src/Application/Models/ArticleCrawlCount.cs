namespace Application.Models;

/// <summary>
/// One (day, provider) bucket's real article count from <see cref="Abstractions.IArticleFingerprintRepository.GetDailyProviderCountsAsync"/> -
/// backs the crawl-report page's "New articles" figures (daily chart + provider table) with an
/// actual count of persisted articles instead of trusting each run's self-reported counter.
/// </summary>
public sealed record ArticleCrawlCount(DateOnly Date, string Provider, int Count);
