using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// The lean duplicate-detection record for one <see cref="NewsArticle"/> - just enough (Url,
/// OriginalGuid, the two hashes, CrawledAt) to answer "have we seen this?" and "did it change?"
/// without loading the full article (Title/Summary/Content/ImageUrl/Tags/Metadata/...). Shares its
/// Id 1:1 with the NewsArticle it fingerprints. See
/// Infrastructure/Persistence/NewsArticleRepository.UpsertAsync for how this replaced querying
/// NewsArticles directly for every dedup check.
/// </summary>
public sealed class ArticleFingerprint
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Mirrored from NewsArticle.Provider - lets the crawl report count real, persisted articles per provider straight from this lean collection.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Mirrored from NewsArticle.SourceType - lets the crawl report filter this collection to one pipeline (RSS/API) the same way it already filters CrawlHistory.</summary>
    public ArticleSourceType SourceType { get; set; } = ArticleSourceType.Rss;

    public string Url { get; set; } = string.Empty;

    /// <summary>The RSS &lt;guid&gt; value as published by the source, mirrored from NewsArticle.OriginalGuid.</summary>
    public string? OriginalGuid { get; set; }

    /// <summary>Hash of Title + PublishedAt - see <see cref="Application.Services.ArticleHasher.ComputeHash"/>.</summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>Hash of Title + Summary + Content + ImageUrl - lets an in-place content change be detected (or ruled out) without loading the full article. See <see cref="Application.Services.ArticleHasher.ComputeContentHash"/>.</summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>Mirrored from NewsArticle.PublishedAt - the source's own publish timestamp, not when this app saw it.</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>The article's original CrawledAt (i.e. its creation date) - preserved across updates the same way NewsArticle.CrawledAt is.</summary>
    public DateTimeOffset CrawledAt { get; set; }

    /// <summary>Mirrored from NewsArticle.UpdatedAt - equal to CrawledAt for a brand new article, refreshed whenever an in-place content change is detected.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
