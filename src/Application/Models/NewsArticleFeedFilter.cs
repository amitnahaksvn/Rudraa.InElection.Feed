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

/// <summary>
/// Query shape for <see cref="Abstractions.INewsArticleRepository.GetFeedAsync"/> - backs the News
/// Feed page's infinite scroll: narrow by pipeline (RSS/API) and/or country, ordered by
/// <see cref="SortBy"/>/<see cref="SortDirection"/>.
///
/// Two mutually-exclusive ways to page: <see cref="Skip"/>/<see cref="Take"/> (offset paging, the
/// original mechanism) or <see cref="Cursor"/>/<see cref="Take"/> (keyset paging). Offset paging is
/// unsound the moment anything is deleted from the collection while a reader is mid-scroll - an
/// article removed anywhere before the current page shifts every later article's position back by
/// one, so the next Skip either re-shows an article already seen (a "duplicate" on screen) or
/// silently drops one that was never shown at all. Confirmed as the actual cause of both symptoms
/// reported against the News Feed page's infinite scroll once article deletion was in the mix. When
/// <see cref="Cursor"/> is set, <see cref="GetFeedAsync"/> ignores <see cref="Skip"/> entirely and
/// instead filters to articles strictly past the given timestamp in the current
/// <see cref="SortDirection"/> (older, for the default Descending/newest-first order) - a value
/// that only ever refers to a real article's own timestamp, so a deletion elsewhere in the
/// collection can't shift it. <see cref="Skip"/> stays as the default so this is backward
/// compatible with a caller that hasn't adopted <see cref="Cursor"/> yet.
/// </summary>
public sealed record NewsArticleFeedFilter(
    ArticleSourceType? SourceType = null,
    string? Country = null,
    int Skip = 0,
    int Take = 20,
    NewsFeedSortBy SortBy = NewsFeedSortBy.PublishedAt,
    NewsFeedSortDirection SortDirection = NewsFeedSortDirection.Descending,
    DateTimeOffset? Cursor = null);
