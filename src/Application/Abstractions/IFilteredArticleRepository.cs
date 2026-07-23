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

    /// <summary>Newest-first page of filtered rows.</summary>
    Task<IReadOnlyList<FilteredArticle>> GetPagedAsync(int skip, int limit, CancellationToken cancellationToken);

    Task<long> CountAsync(CancellationToken cancellationToken);

    /// <summary>Returns false when no row with that id exists.</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
