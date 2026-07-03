namespace Domain.Entities;

/// <summary>
/// Configuration for one dynamically-scheduled RSS feed, stored in the <c>FeedSources</c>
/// collection - unlike the 30+ providers wired up in <c>NewsCrawler.appsettings.json</c> (each of
/// which is a dedicated <c>IRssProvider</c> class, added when a publisher needs spec-tolerance
/// quirks handled), a <see cref="FeedSource"/> needs no code or config-file change at all: adding
/// one is purely a document insert, picked up by <c>DynamicFeedIngestionService</c> through the
/// same generic RSS 2.0 parsing engine <c>BaseRssProvider</c> already uses for every other
/// provider. Intended for straightforward feeds (PIB first); a publisher that needs real
/// spec-tolerance handling (non-standard dates, missing image tags, WAF/UA quirks, etc.) still
/// belongs in the file-based provider system, not here.
/// </summary>
public sealed class FeedSource
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Short unique key, e.g. "PIB" - used as <c>NewsArticle.Provider</c>/<c>CrawlHistory</c> provider tag.</summary>
    public string SourceCode { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string FeedName { get; set; } = string.Empty;

    public string FeedUrl { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Priority { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public int FetchIntervalMinutes { get; set; } = 5;

    public int TimeoutSeconds { get; set; } = 60;

    public DateTimeOffset? LastFetchedOn { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
