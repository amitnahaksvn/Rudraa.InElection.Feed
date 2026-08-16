using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;

namespace WebPlatform;

/// <summary>
/// Shared Hangfire-Mongo storage setup for both WebRssFeed and WebApiFeed - both point at the
/// same connection string/database/"hangfire" prefix, which is what makes their recurring-job
/// storage genuinely shared: either host's own Hangfire dashboard can see both hosts' jobs,
/// regardless of which one actually executes them.
///
/// Deliberately points at a real MongoDB instance (<see cref="Application.Options.HangfireOptions.MongoConnectionString"/>),
/// not the Cosmos DB account every other collection in this app now uses - this codebase briefly
/// ran Hangfire against Cosmos DB's Mongo API (via Hangfire.Mongo's own
/// <c>Hangfire.Mongo.CosmosDB.UseCosmosStorage</c>/<c>CosmosStorageOptions</c> compatibility
/// variant) and hit two confirmed, live incompatibilities: Cosmos DB's Mongo API doesn't support
/// capped collections at all ("Command create failed: Capped collections are not supported."),
/// and its backup-then-migrate schema strategy can be left half-applied and isn't idempotent
/// against its own leftover backup collection ("a collection '...migrationbackup' already
/// exists"). Rather than keep working around Cosmos-specific quirks (Poll instead of Watch,
/// Drop instead of Backup-then-Migrate), Hangfire went back to a real MongoDB instance where none
/// of that applies, while every other repository in this app stays on Cosmos DB.
/// </summary>
public static class HangfireStorageSetup
{
    public static IServiceCollection AddSharedHangfireStorage(
        this IServiceCollection services, string mongoConnectionString, string mongoDatabaseName) =>
        services.AddHangfire(config => config
            .UseMongoStorage(mongoConnectionString, mongoDatabaseName, new MongoStorageOptions
            {
                Prefix = "hangfire",
                // Hangfire.Mongo pings the database synchronously the first time storage is resolved and
                // throws (crashing the whole host) if that single ping doesn't answer within 5s - too
                // fragile against an Atlas cluster's normal latency variance. Mongo connectivity is already
                // verified elsewhere (MongoDb ValidateOnStart-equivalent checks happen at first use).
                CheckConnection = false,
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                }
            }));
}
