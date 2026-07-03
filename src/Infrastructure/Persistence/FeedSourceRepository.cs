using MongoDB.Driver;
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class FeedSourceRepository : IFeedSourceRepository
{
    private readonly IMongoCollection<FeedSource> _collection;

    public FeedSourceRepository(MongoDbContext context)
    {
        _collection = context.FeedSources;
    }

    public async Task<IReadOnlyList<FeedSource>> GetActiveAsync(CancellationToken cancellationToken) =>
        await _collection.Find(f => f.IsActive).ToListAsync(cancellationToken);

    public async Task<FeedSource?> GetBySourceCodeAsync(string sourceCode, CancellationToken cancellationToken) =>
        await _collection.Find(f => f.SourceCode == sourceCode).FirstOrDefaultAsync(cancellationToken);

    public async Task<FeedSource?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await _collection.Find(f => f.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<string> InsertAsync(FeedSource feedSource, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(feedSource, options: null, cancellationToken);
        return feedSource.Id;
    }

    public Task UpdateLastFetchedOnAsync(string id, DateTimeOffset lastFetchedOn, CancellationToken cancellationToken) =>
        _collection.UpdateOneAsync(
            f => f.Id == id,
            Builders<FeedSource>.Update
                .Set(f => f.LastFetchedOn, lastFetchedOn)
                .Set(f => f.UpdatedOn, lastFetchedOn),
            cancellationToken: cancellationToken);

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<FeedSource>>
        {
            new(Builders<FeedSource>.IndexKeys.Ascending(f => f.SourceCode),
                new CreateIndexOptions { Name = "ux_feedsource_sourcecode", Unique = true }),
            new(Builders<FeedSource>.IndexKeys.Ascending(f => f.IsActive),
                new CreateIndexOptions { Name = "ix_feedsource_isactive" }),
            new(Builders<FeedSource>.IndexKeys.Descending(f => f.Priority),
                new CreateIndexOptions { Name = "ix_feedsource_priority" }),
            new(Builders<FeedSource>.IndexKeys.Ascending(f => f.FeedUrl),
                new CreateIndexOptions { Name = "ix_feedsource_feedurl" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
