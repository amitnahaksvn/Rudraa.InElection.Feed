using MongoDB.Bson;
using MongoDB.Driver;
using Application.Abstractions;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class ArticleFingerprintRepository : IArticleFingerprintRepository
{
    private readonly IMongoCollection<ArticleFingerprint> _collection;

    public ArticleFingerprintRepository(MongoDbContext context)
    {
        _collection = context.ArticleFingerprints;
    }

    public Task<ArticleFingerprint?> FindByUrlAsync(string url, CancellationToken cancellationToken) =>
        _collection.Find(f => f.Url == url).FirstOrDefaultAsync(cancellationToken)!;

    public Task<ArticleFingerprint?> FindByOriginalGuidAsync(string originalGuid, CancellationToken cancellationToken) =>
        _collection.Find(f => f.OriginalGuid == originalGuid).FirstOrDefaultAsync(cancellationToken)!;

    public Task<ArticleFingerprint?> FindByHashAsync(string hash, CancellationToken cancellationToken) =>
        _collection.Find(f => f.Hash == hash).FirstOrDefaultAsync(cancellationToken)!;

    public Task InsertAsync(ArticleFingerprint fingerprint, CancellationToken cancellationToken) =>
        _collection.InsertOneAsync(fingerprint, options: null, cancellationToken);

    public Task ReplaceAsync(ArticleFingerprint fingerprint, CancellationToken cancellationToken) =>
        _collection.ReplaceOneAsync(f => f.Id == fingerprint.Id, fingerprint, cancellationToken: cancellationToken);

    /// <summary>
    /// <see cref="ArticleFingerprint.CrawledAt"/> (a DateTimeOffset) is stored via the Mongo C#
    /// driver's default structure representation - <c>{ DateTime, Ticks, Offset }</c>, not a
    /// native BSON Date - so <c>$dateToString</c> has to reach into the nested
    /// <c>crawledAt.DateTime</c> field rather than <c>crawledAt</c> directly. Built as a raw
    /// pipeline (not the typed LINQ aggregation builder) since that nested path has no matching
    /// C# member to bind to.
    /// </summary>
    public async Task<IReadOnlyList<ArticleCrawlCount>> GetDailyProviderCountsAsync(
        ArticleSourceType sourceType, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var stages = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "sourceType", sourceType.ToString() },
                { "crawledAt.DateTime", new BsonDocument { { "$gte", from.UtcDateTime }, { "$lte", to.UtcDateTime } } }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                {
                    "_id", new BsonDocument
                    {
                        { "day", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$crawledAt.DateTime" } }) },
                        { "provider", "$provider" }
                    }
                },
                { "count", new BsonDocument("$sum", 1) }
            })
        };

        var pipeline = PipelineDefinition<ArticleFingerprint, BsonDocument>.Create(stages);
        var results = await _collection.Aggregate(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        return results.Select(r =>
        {
            var id = r["_id"].AsBsonDocument;
            var day = DateOnly.ParseExact(id["day"].AsString, "yyyy-MM-dd");
            return new ArticleCrawlCount(day, id["provider"].AsString, r["count"].ToInt32());
        }).ToList();
    }

    /// <summary>
    /// Same shape as <see cref="GetDailyProviderCountsAsync"/>, bucketed by UpdatedAt instead of
    /// CrawledAt and excluding a fingerprint whose UpdatedAt still equals its CrawledAt (a brand
    /// new article, not an in-place update - see NewsArticleRepository.InsertAsync, which stamps
    /// both to the same value at creation). Deliberately reuses the sourceType-equality prefix of
    /// ix_articlefingerprint_sourcetype_crawledat rather than adding a second dedicated index on
    /// updatedAt.DateTime - one more compound index on a collection this size is a real storage
    /// cost on an already storage-constrained cluster, and this query isn't a hot path.
    /// </summary>
    public async Task<IReadOnlyList<ArticleCrawlCount>> GetDailyProviderUpdatedCountsAsync(
        ArticleSourceType sourceType, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var stages = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "sourceType", sourceType.ToString() },
                { "updatedAt.DateTime", new BsonDocument { { "$gte", from.UtcDateTime }, { "$lte", to.UtcDateTime } } },
                { "$expr", new BsonDocument("$ne", new BsonArray { "$updatedAt.DateTime", "$crawledAt.DateTime" }) }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                {
                    "_id", new BsonDocument
                    {
                        { "day", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$updatedAt.DateTime" } }) },
                        { "provider", "$provider" }
                    }
                },
                { "count", new BsonDocument("$sum", 1) }
            })
        };

        var pipeline = PipelineDefinition<ArticleFingerprint, BsonDocument>.Create(stages);
        var results = await _collection.Aggregate(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        return results.Select(r =>
        {
            var id = r["_id"].AsBsonDocument;
            var day = DateOnly.ParseExact(id["day"].AsString, "yyyy-MM-dd");
            return new ArticleCrawlCount(day, id["provider"].AsString, r["count"].ToInt32());
        }).ToList();
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<ArticleFingerprint>>
        {
            new(Builders<ArticleFingerprint>.IndexKeys.Ascending(f => f.Url),
                new CreateIndexOptions { Unique = true, Name = "ux_articlefingerprint_url" }),
            new(Builders<ArticleFingerprint>.IndexKeys.Ascending(f => f.Hash),
                new CreateIndexOptions { Unique = true, Name = "ux_articlefingerprint_hash" }),
            new(Builders<ArticleFingerprint>.IndexKeys.Ascending(f => f.OriginalGuid),
                new CreateIndexOptions { Name = "ix_articlefingerprint_originalguid" }),
            // Backs GetDailyProviderCountsAsync's own $match (sourceType equality + crawledAt.DateTime
            // range) - the crawl-report page's own query, run every time that page loads. Indexed on
            // the raw "crawledAt.DateTime" path (not the typed CrawledAt member, which would index the
            // whole {DateTime,Ticks,Offset} structure) so it actually matches what that $match queries.
            new(Builders<ArticleFingerprint>.IndexKeys.Combine(
                    Builders<ArticleFingerprint>.IndexKeys.Ascending(f => f.SourceType),
                    Builders<ArticleFingerprint>.IndexKeys.Ascending("crawledAt.DateTime")),
                new CreateIndexOptions { Name = "ix_articlefingerprint_sourcetype_crawledat" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
