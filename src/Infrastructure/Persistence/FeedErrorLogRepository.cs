using MongoDB.Driver;
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class FeedErrorLogRepository : IFeedErrorLogRepository
{
    private readonly IMongoCollection<FeedErrorLog> _collection;

    public FeedErrorLogRepository(MongoDbContext context)
    {
        _collection = context.FeedErrorLogs;
    }

    public Task InsertAsync(FeedErrorLog errorLog, CancellationToken cancellationToken) =>
        _collection.InsertOneAsync(errorLog, options: null, cancellationToken);

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<FeedErrorLog>>
        {
            new(Builders<FeedErrorLog>.IndexKeys.Ascending(e => e.FeedSourceId),
                new CreateIndexOptions { Name = "ix_feederrorlog_feedsourceid" }),
            new(Builders<FeedErrorLog>.IndexKeys.Descending(e => e.OccurredOn),
                new CreateIndexOptions { Name = "ix_feederrorlog_occurredon" }),
            new(Builders<FeedErrorLog>.IndexKeys.Ascending(e => e.Resolved),
                new CreateIndexOptions { Name = "ix_feederrorlog_resolved" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
