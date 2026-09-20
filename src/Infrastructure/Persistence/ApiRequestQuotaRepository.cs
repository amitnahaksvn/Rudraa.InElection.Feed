using MongoDB.Bson;
using MongoDB.Driver;
using Application.Abstractions;
using Infrastructure.Cosmos;

namespace Infrastructure.Persistence;

/// <summary>
/// One document per provider per UTC day (<c>_id</c> = "{provider}:{yyyy-MM-dd}", <c>count</c>).
/// The increment is a single atomic upsert whose filter requires <c>count &lt; limit</c>: when the
/// budget is spent the filter matches nothing, so the upsert tries to insert an existing _id and
/// fails with a duplicate-key error - which is exactly the "denied" signal, with no
/// read-then-write race between concurrent instances.
/// </summary>
public sealed class ApiRequestQuotaRepository : IApiRequestQuota
{
    private const string CollectionName = "ApiRequestQuotas";

    private readonly IMongoCollection<BsonDocument> _collection;

    public ApiRequestQuotaRepository(CosmosDbContext context)
    {
        _collection = context.Database.GetCollection<BsonDocument>(CollectionName);
    }

    public async Task<bool> TryConsumeAsync(string provider, int dailyLimit, CancellationToken cancellationToken)
    {
        var id = $"{provider}:{DateTime.UtcNow:yyyy-MM-dd}";
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            Builders<BsonDocument>.Filter.Lt("count", dailyLimit));
        var update = Builders<BsonDocument>.Update.Inc("count", 1);

        try
        {
            await _collection.FindOneAndUpdateAsync(
                filter, update, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true }, cancellationToken);
            return true;
        }
        catch (MongoCommandException ex) when (ex.Code == 11000)
        {
            return false;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
