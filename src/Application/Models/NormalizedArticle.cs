using Domain.Enums;

namespace Application.Models;

/// <summary>
/// Common shape every <see cref="Abstractions.IRssProvider"/>/<see cref="Abstractions.INewsApiProvider"/>
/// must normalize its items into, before the crawler orchestrator persists them. Providers never
/// touch MongoDB directly. A <c>sealed record</c> (not a plain class) specifically so
/// <see cref="Infrastructure.NewsApiProviders.BaseNewsApiProvider"/> can stamp
/// <see cref="SourceType"/> onto every article a concrete provider's parser returns via a single
/// <c>with</c> expression, instead of every JSON-API provider repeating that assignment itself.
/// </summary>
public sealed record NormalizedArticle
{
    public required string Provider { get; init; }

    public required string FeedName { get; init; }

    public required string Category { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Content { get; init; }

    public required string Url { get; init; }

    public string? OriginalGuid { get; init; }

    public string? Author { get; init; }

    public string Language { get; init; } = "hi";

    /// <summary>Country the publisher is based in - see <see cref="Options.RssFeedOptions.Country"/>. Defaults to "India" for every provider that predates the first international additions.</summary>
    public string Country { get; init; } = "India";

    public string? ImageUrl { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public List<string> Tags { get; init; } = [];

    public required string Source { get; init; }

    /// <summary>RSS/Atom by default - every RSS provider (including the Mongo <c>FeedSource</c> pipeline and YouTube's channel feeds) leaves this unset; JSON-API providers get it stamped to <see cref="ArticleSourceType.Api"/> centrally by <c>BaseNewsApiProvider</c>.</summary>
    public ArticleSourceType SourceType { get; init; } = ArticleSourceType.Rss;
}
