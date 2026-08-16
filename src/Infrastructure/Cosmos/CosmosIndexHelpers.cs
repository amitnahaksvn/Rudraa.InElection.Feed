using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Cosmos;

/// <summary>
/// Azure Cosmos DB's Mongo API (RU-based) refuses to create a unique index on a collection that
/// already has documents - "Cannot create unique index when collection contains documents" - even
/// when there are no actual duplicate values under that key. That's a hard platform limitation,
/// not a data conflict: real MongoDB allows this as long as the existing data has no duplicates,
/// Cosmos just doesn't support it at all against a non-empty collection. Every repository calling
/// this already enforces uniqueness at the application layer too (upsert-by-filter via
/// <c>Builders.Filter</c>-based <c>UpdateOneAsync(..., IsUpsert = true)</c>, or an explicit
/// pre-insert existence check), so the database-level unique index is defense-in-depth, not the
/// only thing preventing duplicates - safe to skip (with a warning) rather than crash the whole
/// host on startup when Cosmos won't allow it.
/// </summary>
public static class CosmosIndexHelpers
{
    public static async Task TryCreateUniqueIndexAsync<T>(
        IMongoIndexManager<T> indexes, CreateIndexModel<T> model, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            await indexes.CreateOneAsync(model, cancellationToken: cancellationToken);
        }
        catch (MongoCommandException ex) when (ex.Message.Contains(
            "Cannot create unique index when collection contains documents", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Skipped unique index '{IndexName}' - Cosmos DB's Mongo API won't create a unique index on a non-empty collection. Uniqueness is still enforced at the application layer.",
                model.Options.Name);
        }
    }
}
