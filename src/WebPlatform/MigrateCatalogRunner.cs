using Infrastructure.Seed;

namespace WebPlatform;

/// <summary>
/// Backs `dotnet run -- --migrate-catalog`: the one-time, by-hand invocation of
/// <see cref="CrawlCatalogMigrationSeeder"/> that copies the legacy JSON provider catalog into
/// the database-backed CrawlCountry/ProviderSchedule/CrawlFeed collections. Same throwaway-
/// ServiceProvider shape as <see cref="InitDbRunner"/>, for the same reason - the caller returns
/// immediately after this runs, so builder.Build()/app.Run() never executes.
/// </summary>
public static class MigrateCatalogRunner
{
    public static async Task RunAsync(IServiceCollection services)
    {
#pragma warning disable ASP0000
        using var provider = services.BuildServiceProvider();
#pragma warning restore ASP0000
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("MigrateCatalog");

        logger.LogInformation("--migrate-catalog: migrating legacy JSON provider catalog into the database");
        var seeder = provider.GetRequiredService<CrawlCatalogMigrationSeeder>();
        await seeder.MigrateAsync(CancellationToken.None);
        logger.LogInformation("--migrate-catalog: done");
    }
}
