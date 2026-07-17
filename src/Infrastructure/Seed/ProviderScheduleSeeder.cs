using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Options;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Seed;

/// <summary>
/// One-time migration bootstrap for <see cref="ProviderSchedule"/>: for every RSS/API provider
/// currently configured in <c>NewsCrawler.appsettings.json</c>/<c>NewsApiCrawler</c>, seeds a
/// document with that provider's file-configured Enabled/Cron if (and only if) no document exists
/// yet for that (Pipeline, Provider) pair - so re-running this on every startup never overwrites a
/// since-edited schedule. A provider added to the file after this has already run once just gets
/// seeded on the next startup that sees it for the first time.
/// </summary>
public sealed class ProviderScheduleSeeder
{
    // Same concurrency this codebase already settled on for HangfireRecurringJobRegistrar's own
    // per-provider Mongo round trips - independent upserts, no reason to run them one at a time.
    private const int SeedConcurrency = 32;

    // Every provider (RSS and API alike, 600+ of them) shared this exact literal cron until the
    // "stop crawling everything every 20 minutes" pass - see UpgradeLegacyDefaultCronsAsync below.
    private const string LegacyDefaultCron = "*/20 * * * *";

    private readonly IProviderScheduleRepository _schedules;
    private readonly NewsCrawlerOptions _rssOptions;
    private readonly NewsApiCrawlerOptions _apiOptions;
    private readonly ILogger<ProviderScheduleSeeder> _logger;

    public ProviderScheduleSeeder(
        IProviderScheduleRepository schedules,
        IOptions<NewsCrawlerOptions> rssOptions,
        IOptions<NewsApiCrawlerOptions> apiOptions,
        ILogger<ProviderScheduleSeeder> logger)
    {
        _schedules = schedules;
        _rssOptions = rssOptions.Value;
        _apiOptions = apiOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var rssSchedules = _rssOptions.Countries
            .SelectMany(c => c.Providers.Select(p => BuildSchedule(CrawlPipeline.Rss, c.Name, p.Name, p.Enabled, p.Cron, now)))
            .ToList();

        var apiSchedules = _apiOptions.Countries
            .SelectMany(c => c.Providers.Select(p => BuildSchedule(CrawlPipeline.Api, c.Name, p.Name, p.Enabled, p.Cron, now)))
            .ToList();

        var allSchedules = rssSchedules.Concat(apiSchedules).ToList();

        await Parallel.ForEachAsync(
            allSchedules,
            new ParallelOptions { MaxDegreeOfParallelism = SeedConcurrency },
            (schedule, ct) => new ValueTask(_schedules.SeedIfMissingAsync(schedule, ct)));

        _logger.LogInformation(
            "Seeded ProviderSchedule bootstrap check for {Count} providers ({Rss} RSS, {Api} API)",
            allSchedules.Count, rssSchedules.Count, apiSchedules.Count);
    }

    private static ProviderSchedule BuildSchedule(
        CrawlPipeline pipeline, string country, string provider, bool enabled, string cron, DateTimeOffset now) => new()
    {
        Pipeline = pipeline,
        Provider = provider,
        Country = country,
        Enabled = enabled,
        Cron = cron,
        TimeZone = "UTC",
        UpdatedAt = now
    };

    /// <summary>
    /// One-time follow-up migration for documents <see cref="SeedAsync"/> already created before the
    /// "stop crawling everything every 20 minutes" config rewrite: a document whose Cron is still
    /// exactly the old shared literal gets moved onto that provider's new file-configured Cron.
    /// Never touches a document a user has since edited away from the legacy default (Provider
    /// Management page or otherwise) - those are left exactly as chosen, same non-destructive
    /// guarantee <see cref="SeedAsync"/> already gives for Enabled/Cron generally.
    /// </summary>
    public async Task UpgradeLegacyDefaultCronsAsync(CancellationToken cancellationToken)
    {
        var rssCronByProvider = _rssOptions.Countries
            .SelectMany(c => c.Providers)
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Cron, StringComparer.OrdinalIgnoreCase);

        var apiCronByProvider = _apiOptions.Countries
            .SelectMany(c => c.Providers)
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Cron, StringComparer.OrdinalIgnoreCase);

        var rssUpgraded = await UpgradeAsync(CrawlPipeline.Rss, rssCronByProvider, cancellationToken);
        var apiUpgraded = await UpgradeAsync(CrawlPipeline.Api, apiCronByProvider, cancellationToken);

        _logger.LogInformation(
            "Upgraded {Count} provider schedules off the legacy '{LegacyCron}' default cron ({Rss} RSS, {Api} API)",
            rssUpgraded + apiUpgraded, LegacyDefaultCron, rssUpgraded, apiUpgraded);
    }

    private async Task<int> UpgradeAsync(
        CrawlPipeline pipeline, IReadOnlyDictionary<string, string> cronByProvider, CancellationToken cancellationToken)
    {
        var existing = await _schedules.GetAllAsync(pipeline, cancellationToken);

        var toUpgrade = existing
            .Where(s => s.Cron == LegacyDefaultCron
                && cronByProvider.TryGetValue(s.Provider, out var newCron)
                && newCron != LegacyDefaultCron)
            .ToList();

        var now = DateTimeOffset.UtcNow;

        await Parallel.ForEachAsync(
            toUpgrade,
            new ParallelOptions { MaxDegreeOfParallelism = SeedConcurrency },
            async (schedule, ct) =>
            {
                schedule.Cron = cronByProvider[schedule.Provider];
                schedule.UpdatedAt = now;
                await _schedules.UpsertAsync(schedule, ct);
            });

        return toUpgrade.Count;
    }
}
