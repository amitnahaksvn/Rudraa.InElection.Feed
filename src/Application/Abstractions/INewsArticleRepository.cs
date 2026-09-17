using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>Upsert result reported back to the crawler orchestrator for logging/metrics.</summary>
public enum ArticleUpsertOutcome
{
    Inserted,
    DuplicateSkipped
}

public interface INewsArticleRepository
{
    /// <summary>
    /// Inserts a brand new article, or reports a duplicate skip when the incoming article matches
    /// an existing one - an existing article is never modified in place, regardless of whether its
    /// content differs from what's stored. Duplicate detection order: Url, then OriginalGuid, then
    /// Hash - resolved entirely against the lean <see cref="Domain.Entities.ArticleFingerprint"/>
    /// collection (<see cref="IArticleFingerprintRepository"/>), so a duplicate skip never loads
    /// the full article.
    /// </summary>
    Task<ArticleUpsertOutcome> UpsertAsync(NewsArticle article, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int count, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetByProviderAsync(string provider, int count, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetByCategoryAsync(string category, int count, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> SearchAsync(string query, int count, CancellationToken cancellationToken);

    /// <summary>Newest-first, optionally narrowed to one pipeline (RSS/API) and/or one country - backs the News Feed page's infinite scroll.</summary>
    Task<IReadOnlyList<NewsArticle>> GetFeedAsync(NewsArticleFeedFilter filter, CancellationToken cancellationToken);

    /// <summary>Total articles matching <paramref name="filter"/>'s pipeline/country narrowing (its Skip/Take are ignored) - backs the News Feed page's total-count header.</summary>
    Task<long> CountFeedAsync(NewsArticleFeedFilter filter, CancellationToken cancellationToken);

    /// <summary>Every distinct, non-empty country currently represented among active articles (optionally narrowed to one pipeline) - backs the News Feed page's country filter.</summary>
    Task<IReadOnlyList<string>> GetDistinctCountriesAsync(ArticleSourceType? sourceType, CancellationToken cancellationToken);

    /// <summary>
    /// Permanently removes the articles with these ids - backs the News Feed page's per-card delete
    /// and its multi-select bulk delete, the same call either way. Ids that don't match any document
    /// are silently ignored rather than erroring. A hard delete, not a soft one: this used to only
    /// flip <see cref="Domain.Entities.NewsArticle.IsActive"/> to false, but that left the document
    /// (and every duplicate-looking copy a user complained about) sitting in the collection forever,
    /// which is what the user actually meant by "delete". The matching
    /// <see cref="Domain.Entities.ArticleFingerprint"/> is deliberately left in place regardless -
    /// so a deleted article doesn't come back the next time its source feed is crawled again, the
    /// one part of the old soft-delete's reasoning that still applies to a real delete.
    /// </summary>
    Task<long> DeleteManyAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken);

    /// <summary>Ensures the PublishedAt, Provider and Category indexes exist - Url/OriginalGuid/Hash uniqueness now lives on <see cref="IArticleFingerprintRepository"/> instead.</summary>
    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
