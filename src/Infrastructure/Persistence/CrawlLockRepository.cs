using MongoDB.Bson;
using MongoDB.Driver;
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

/// <summary>
/// Mongo-backed distributed lock. Reclaiming an expired lock and creating a brand new one are
/// each single atomic Mongo operations, so concurrent crawler instances can race safely:
/// exactly one will win.
/// </summary>
public sealed class CrawlLockRepository : ICrawlLockRepository
{
    private readonly IMongoCollection<CrawlLock> _collection;

    public CrawlLockRepository(MongoDbContext context)
    {
        _collection = context.CrawlLocks;
    }

    public async Task<bool> TryAcquireAsync(string lockName, string owner, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(ttl);

        var reclaimFilter = Builders<CrawlLock>.Filter.And(
            Builders<CrawlLock>.Filter.Eq(l => l.Id, lockName),
            Builders<CrawlLock>.Filter.Lt(l => l.ExpiresAt, now));

        var update = Builders<CrawlLock>.Update
            .Set(l => l.Owner, owner)
            .Set(l => l.AcquiredAt, now)
            .Set(l => l.ExpiresAt, expiresAt);

        var reclaimed = await _collection.FindOneAndUpdateAsync(reclaimFilter, update, cancellationToken: cancellationToken);
        if (reclaimed is not null)
        {
            return true;
        }

        try
        {
            await _collection.InsertOneAsync(
                new CrawlLock { Id = lockName, Owner = owner, AcquiredAt = now, ExpiresAt = expiresAt },
                options: null,
                cancellationToken);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Another instance is currently holding a still-valid lock.
            return false;
        }
    }

    public async Task ReleaseAsync(string lockName, string owner, CancellationToken cancellationToken)
    {
        var filter = Builders<CrawlLock>.Filter.And(
            Builders<CrawlLock>.Filter.Eq(l => l.Id, lockName),
            Builders<CrawlLock>.Filter.Eq(l => l.Owner, owner));

        await _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
    }

    private const string TtlIndexName = "ttl_crawllock_expiresat";

    // The TTL index's key path depends on how CrawlLock.ExpiresAt happens to get BSON-serialized -
    // an older driver/serialization convention wrote DateTimeOffset as a nested {DateTime, Ticks}
    // document (key path "expiresAt.DateTime"), while the driver in use today writes it as a plain
    // top-level date ("expiresAt"). MongoDB's createIndexes rejects an index that already exists
    // under the same name with a different key spec rather than updating it in place (the same
    // "options never update via plain createIndexes" gap RssRawResponseRepository already works
    // around for its own TTL index's expireAfterSeconds) - so drop and recreate whenever the
    // existing index's key path doesn't match what gets written today.
    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var existingIndexes = await (await _collection.Indexes.ListAsync(cancellationToken)).ToListAsync(cancellationToken);
        var existing = existingIndexes.FirstOrDefault(i => i["name"].AsString == TtlIndexName);

        if (existing is not null && !existing["key"].AsBsonDocument.Contains("expiresAt"))
        {
            await _collection.Indexes.DropOneAsync(TtlIndexName, cancellationToken);
            existing = null;
        }

        if (existing is null)
        {
            var model = new CreateIndexModel<CrawlLock>(
                Builders<CrawlLock>.IndexKeys.Ascending(l => l.ExpiresAt),
                new CreateIndexOptions { Name = TtlIndexName, ExpireAfter = TimeSpan.Zero });

            await _collection.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);
        }
    }
}
