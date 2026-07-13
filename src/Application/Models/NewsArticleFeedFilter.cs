using Domain.Enums;

namespace Application.Models;

/// <summary>Which timestamp the News Feed page's infinite scroll is ordered by - see <see cref="NewsArticleFeedFilter.SortBy"/>.</summary>
public enum NewsFeedSortBy
{
    /// <summary>When the source published the article - falls back to <see cref="CrawledAt"/> for the minority of articles/feeds with no publish date at all.</summary>
    PublishedAt,

    /// <summary>When this app fetched the article - always populated, so no fallback needed.</summary>
    CrawledAt
}

/// <summary>
/// Which way <see cref="NewsArticleFeedFilter.SortBy"/> orders the feed. Descending is the default
/// (and first enum member, so it's also what an omitted/default-constructed filter gets) since
/// "newest first" is what every reader expects walking in; Ascending exists for someone
/// deliberately reading oldest-first (e.g. reconstructing a story's timeline).
/// </summary>
public enum NewsFeedSortDirection
{
    Descending,
    Ascending
}

/// <summary>Query shape for <see cref="Abstractions.INewsArticleRepository.GetFeedAsync"/> - backs the News Feed page's infinite scroll: narrow by pipeline (RSS/API) and/or country, page via Skip/Take, ordered by <see cref="SortBy"/>/<see cref="SortDirection"/>.</summary>
public sealed record NewsArticleFeedFilter(
    ArticleSourceType? SourceType = null,
    string? Country = null,
    int Skip = 0,
    int Take = 20,
    NewsFeedSortBy SortBy = NewsFeedSortBy.PublishedAt,
    NewsFeedSortDirection SortDirection = NewsFeedSortDirection.Descending);
