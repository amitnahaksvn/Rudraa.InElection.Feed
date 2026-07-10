using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>Upsert result reported back to the crawler orchestrator for logging/metrics.</summary>
public enum ArticleUpsertOutcome
{
    Inserted,
    Updated,
    DuplicateSkipped
}

public interface INewsArticleRepository
{
    Task<NewsArticle?> FindByUrlAsync(string url, CancellationToken cancellationToken);

    Task<NewsArticle?> FindByOriginalGuidAsync(string originalGuid, CancellationToken cancellationToken);

    Task<NewsArticle?> FindByHashAsync(string hash, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts a brand new article, updates an existing one whose content changed, or reports
    /// a duplicate skip when the incoming article matches an existing one with no changes.
    /// Duplicate detection order: Url, then OriginalGuid, then Hash.
    /// </summary>
    Task<ArticleUpsertOutcome> UpsertAsync(NewsArticle article, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int count, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetByProviderAsync(string provider, int count, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetByCategoryAsync(string category, int count, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> SearchAsync(string query, int count, CancellationToken cancellationToken);

    /// <summary>Newest-first, optionally narrowed to one pipeline (RSS/API) and/or one country - backs the News Feed page's infinite scroll.</summary>
    Task<IReadOnlyList<NewsArticle>> GetFeedAsync(NewsArticleFeedFilter filter, CancellationToken cancellationToken);

    /// <summary>Every distinct, non-empty country currently represented among active articles (optionally narrowed to one pipeline) - backs the News Feed page's country filter.</summary>
    Task<IReadOnlyList<string>> GetDistinctCountriesAsync(ArticleSourceType? sourceType, CancellationToken cancellationToken);

    /// <summary>Ensures the Url (unique), OriginalGuid, Hash, PublishedAt, Provider and Category indexes exist.</summary>
    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
