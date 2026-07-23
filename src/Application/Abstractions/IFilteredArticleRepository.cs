using Application.Models;
using Domain.Entities;

namespace Application.Abstractions;

/// <summary>Persistence for <see cref="FilteredArticle"/> - the log of articles excluded by the political-category allowlist (see <c>Application.Options.NewsFilterOptions</c>).</summary>
public interface IFilteredArticleRepository
{
    /// <summary>
    /// Race-safe fingerprint-then-document insert, the same fingerprint-first pattern
    /// <c>NewsArticleRepository.InsertAsync</c> uses for real articles - reserves an
    /// <see cref="Domain.Entities.ArticleFingerprint"/> for this Url/OriginalGuid/Hash first (its
    /// unique indexes decide any concurrent-provider race) and only inserts the FilteredArticle,
    /// sharing the fingerprint's own Id, once that succeeds. The caller (<c>ArticlePersister</c>)
    /// is expected to have already checked <c>IArticleFingerprintRepository.FindDuplicateAsync</c>
    /// beforehand - this only closes the narrow race window between that check and this insert,
    /// the same accepted trade-off already documented on NewsArticleRepository.InsertAsync.
    /// </summary>
    Task InsertAsync(
        FilteredArticle article,
        string url,
        string? originalGuid,
        string hash,
        DateTimeOffset? publishedAt,
        CancellationToken cancellationToken);

    /// <summary>Newest-first page of filtered rows matching <paramref name="filter"/>.</summary>
    Task<IReadOnlyList<FilteredArticle>> GetPagedAsync(FilteredArticleFilter filter, int skip, int limit, CancellationToken cancellationToken);

    /// <summary>Total rows matching the same narrowing as <see cref="GetPagedAsync"/> (skip/limit are irrelevant here) - backs the admin page's pagination total.</summary>
    Task<long> CountAsync(FilteredArticleFilter filter, CancellationToken cancellationToken);

    /// <summary>Every distinct <see cref="FilteredArticle.Provider"/> currently logged - backs the admin page's provider filter.</summary>
    Task<IReadOnlyList<string>> GetDistinctProvidersAsync(CancellationToken cancellationToken);

    /// <summary>Every distinct <see cref="FilteredArticle.Category"/> currently logged - backs the admin page's category filter.</summary>
    Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken);

    /// <summary>Deletes one or more rows by id - backs both the admin page's per-row delete button and its multi-select bulk delete. Returns how many were actually found and deleted.</summary>
    Task<long> DeleteManyAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
