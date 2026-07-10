using MongoDB.Bson;
using MongoDB.Driver;
using Application.Abstractions;
using Application.Models;
using Domain.Entities;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class CrawlHistoryRepository : ICrawlHistoryRepository
{
    private readonly IMongoCollection<CrawlHistory> _collection;

    public CrawlHistoryRepository(MongoDbContext context)
    {
        _collection = context.CrawlHistory;
    }

    public async Task<string> InsertAsync(CrawlHistory history, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(history.Id))
        {
            history.Id = ObjectId.GenerateNewId().ToString();
        }

        await _collection.InsertOneAsync(history, options: null, cancellationToken);
        return history.Id;
    }

    public Task UpdateAsync(CrawlHistory history, CancellationToken cancellationToken) =>
        _collection.ReplaceOneAsync(h => h.Id == history.Id, history, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<CrawlHistory>> GetFilteredAsync(CrawlHistoryFilter filter, CancellationToken cancellationToken)
    {
        var builder = Builders<CrawlHistory>.Filter;
        var clauses = new List<FilterDefinition<CrawlHistory>>();

        if (filter.Pipeline is { } pipeline)
        {
            clauses.Add(builder.Eq(h => h.Pipeline, pipeline));
        }

        if (!string.IsNullOrWhiteSpace(filter.Provider))
        {
            clauses.Add(builder.AnyEq(h => h.Providers, filter.Provider));
        }

        if (filter.From is { } from)
        {
            clauses.Add(builder.Gte(h => h.StartTime, from));
        }

        if (filter.To is { } to)
        {
            clauses.Add(builder.Lte(h => h.StartTime, to));
        }

        var combined = clauses.Count == 0 ? FilterDefinition<CrawlHistory>.Empty : builder.And(clauses);

        return await _collection.Find(combined)
            .SortByDescending(h => h.StartTime)
            .Skip(filter.Skip)
            .Limit(filter.Take)
            .ToListAsync(cancellationToken);
    }

    public async Task<CrawlHistory?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await _collection.Find(h => h.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new[]
        {
            new CreateIndexModel<CrawlHistory>(
                Builders<CrawlHistory>.IndexKeys.Descending(h => h.StartTime),
                new CreateIndexOptions { Name = "ix_crawlhistory_starttime" }),

            // Backs the crawl-report page's primary access pattern: "every Rss/Api run in a date
            // range" - filtering on Pipeline first, then sorting/ranging on StartTime, matches this
            // compound index's key order.
            new CreateIndexModel<CrawlHistory>(
                Builders<CrawlHistory>.IndexKeys.Ascending(h => h.Pipeline).Descending(h => h.StartTime),
                new CreateIndexOptions { Name = "ix_crawlhistory_pipeline_starttime" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken: cancellationToken);
    }
}
