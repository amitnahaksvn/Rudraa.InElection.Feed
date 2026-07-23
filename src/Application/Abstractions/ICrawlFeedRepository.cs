using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>Persistence for <see cref="CrawlFeed"/> - the database-backed replacement for <c>RssFeedOptions</c>/<c>NewsApiEndpointOptions</c>.</summary>
public interface ICrawlFeedRepository
{
    Task<IReadOnlyList<CrawlFeed>> GetAllAsync(CrawlPipeline pipeline, CancellationToken cancellationToken);

    Task<IReadOnlyList<CrawlFeed>> GetByProviderAsync(CrawlPipeline pipeline, string provider, CancellationToken cancellationToken);

    Task<CrawlFeed?> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>Inserts a brand-new feed, assigning its Id, and returns the assigned Id.</summary>
    Task<string> CreateAsync(CrawlFeed feed, CancellationToken cancellationToken);

    /// <summary>Full overwrite of an existing feed's editable fields, matched by <see cref="CrawlFeed.Id"/>. Returns false when no row with that id exists.</summary>
    Task<bool> UpdateAsync(CrawlFeed feed, CancellationToken cancellationToken);

    /// <summary>Returns false when no row with that id exists.</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
