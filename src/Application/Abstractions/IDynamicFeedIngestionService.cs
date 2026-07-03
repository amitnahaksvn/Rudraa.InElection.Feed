namespace Application.Abstractions;

/// <summary>
/// Downloads, parses, deduplicates, and persists one <see cref="Domain.Entities.FeedSource"/>-driven
/// feed - the generic counterpart to <see cref="INewsCrawlerService"/>'s file-configured provider
/// crawl, invoked per feed rather than per provider since a <see cref="Domain.Entities.FeedSource"/>
/// document is itself both.
/// </summary>
public interface IDynamicFeedIngestionService
{
    Task RunAsync(string feedSourceId, CancellationToken cancellationToken);
}
