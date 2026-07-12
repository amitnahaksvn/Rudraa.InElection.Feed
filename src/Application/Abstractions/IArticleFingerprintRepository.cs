using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>
/// The lean duplicate-detection lookup <c>INewsArticleRepository.UpsertAsync</c> checks before
/// ever touching the full NewsArticles collection - see <see cref="ArticleFingerprint"/> for why
/// this exists as its own collection instead of querying NewsArticles directly.
/// </summary>
public interface IArticleFingerprintRepository
{
    Task<ArticleFingerprint?> FindByUrlAsync(string url, CancellationToken cancellationToken);

    Task<ArticleFingerprint?> FindByOriginalGuidAsync(string originalGuid, CancellationToken cancellationToken);

    Task<ArticleFingerprint?> FindByHashAsync(string hash, CancellationToken cancellationToken);

    Task InsertAsync(ArticleFingerprint fingerprint, CancellationToken cancellationToken);

    Task ReplaceAsync(ArticleFingerprint fingerprint, CancellationToken cancellationToken);

    /// <summary>
    /// Real per-(day, provider) article counts within a date range, grouped straight out of this
    /// lean collection's own CrawledAt - backs the crawl-report page's "New articles" figures.
    /// </summary>
    Task<IReadOnlyList<ArticleCrawlCount>> GetDailyProviderCountsAsync(
        ArticleSourceType sourceType, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>
    /// Real per-(day, provider) counts of articles whose content actually changed after creation
    /// (UpdatedAt differs from CrawledAt) within a date range, bucketed by UpdatedAt - backs the
    /// crawl-report page's "Updated articles" figures the same way GetDailyProviderCountsAsync
    /// backs "New articles". "Duplicate" has no equivalent here - a duplicate skip never writes a
    /// fingerprint at all, so that figure stays sourced from CrawlHistory's own run counters.
    /// </summary>
    Task<IReadOnlyList<ArticleCrawlCount>> GetDailyProviderUpdatedCountsAsync(
        ArticleSourceType sourceType, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
