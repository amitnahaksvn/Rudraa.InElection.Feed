using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>Database-backed Enabled/Cron/TimeZone for RSS and JSON-API providers - see <see cref="ProviderSchedule"/> for why this replaced <c>NewsCrawler.appsettings.json</c>'s own Enabled/Cron as the source of truth.</summary>
public interface IProviderScheduleRepository
{
    Task<IReadOnlyList<ProviderSchedule>> GetAllAsync(CrawlPipeline pipeline, CancellationToken cancellationToken);

    Task<ProviderSchedule?> GetAsync(CrawlPipeline pipeline, string provider, CancellationToken cancellationToken);

    /// <summary>Inserts a document with these values only if one doesn't already exist for this (Pipeline, Provider) pair - the one-time migration bootstrap from appsettings; never overwrites an existing (possibly user-edited) record.</summary>
    Task SeedIfMissingAsync(ProviderSchedule schedule, CancellationToken cancellationToken);

    /// <summary>Creates or fully overwrites one provider's Enabled/Cron/TimeZone - the user-driven edit path from the Provider Management page.</summary>
    Task UpsertAsync(ProviderSchedule schedule, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
