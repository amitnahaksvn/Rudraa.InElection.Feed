using Domain.Entities;

namespace Application.Abstractions;

/// <summary>Persistence for <see cref="FeedErrorLog"/> - every exception raised ingesting a <see cref="Domain.Entities.FeedSource"/>.</summary>
public interface IFeedErrorLogRepository
{
    Task InsertAsync(FeedErrorLog errorLog, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
