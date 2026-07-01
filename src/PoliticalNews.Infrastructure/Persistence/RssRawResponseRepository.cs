using MongoDB.Driver;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Domain.Entities;
using PoliticalNews.Infrastructure.Mongo;

namespace PoliticalNews.Infrastructure.Persistence;

public sealed class RssRawResponseRepository : IRssRawResponseRepository
{
    private readonly IMongoCollection<RssRawResponse> _collection;

    public RssRawResponseRepository(MongoDbContext context)
    {
        _collection = context.RssRawResponses;
    }

    public Task InsertAsync(RssRawResponse response, CancellationToken cancellationToken) =>
        _collection.InsertOneAsync(response, options: null, cancellationToken);

    public async Task<IReadOnlyList<RssRawResponse>> GetRecentAsync(
        string provider, string feedName, int count, CancellationToken cancellationToken) =>
        await _collection
            .Find(r => r.Provider == provider && r.FeedName == feedName)
            .SortByDescending(r => r.FetchedAt)
            .Limit(count)
            .ToListAsync(cancellationToken);

    public async Task EnsureIndexesAsync(TimeSpan retention, CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<RssRawResponse>>
        {
            new(Builders<RssRawResponse>.IndexKeys.Ascending(r => r.Provider).Ascending(r => r.FeedName),
                new CreateIndexOptions { Name = "ix_rawresponse_provider_feed" }),
            new(Builders<RssRawResponse>.IndexKeys.Descending(r => r.FetchedAt),
                new CreateIndexOptions { Name = "ix_rawresponse_fetchedat" }),
            new(Builders<RssRawResponse>.IndexKeys.Ascending(r => r.ContentHash),
                new CreateIndexOptions { Name = "ix_rawresponse_contenthash" }),
            new(Builders<RssRawResponse>.IndexKeys.Ascending(r => r.CreatedAt),
                new CreateIndexOptions { Name = "ttl_rawresponse_createdat", ExpireAfter = retention })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
