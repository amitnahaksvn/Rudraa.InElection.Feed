using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>Database-backed catalog record (schedule + request-shape fields) for RSS and JSON-API providers - see <see cref="ProviderSchedule"/>.</summary>
public interface IProviderScheduleRepository
{
    Task<IReadOnlyList<ProviderSchedule>> GetAllAsync(CrawlPipeline pipeline, CancellationToken cancellationToken);

    Task<ProviderSchedule?> GetAsync(CrawlPipeline pipeline, string provider, CancellationToken cancellationToken);

    /// <summary>Inserts a document with these values only if one doesn't already exist for this (Pipeline, Provider) pair - the one-time migration bootstrap from the legacy JSON files; never overwrites an existing (possibly user-edited) record.</summary>
    Task SeedIfMissingAsync(ProviderSchedule schedule, CancellationToken cancellationToken);

    /// <summary>Creates a brand-new provider or fully overwrites an existing one's catalog record - the user-driven add/edit path from the Provider Management page.</summary>
    Task UpsertAsync(ProviderSchedule schedule, CancellationToken cancellationToken);

    /// <summary>
    /// Patches only <see cref="ProviderSchedule.SaveRawResponses"/>/<see cref="ProviderSchedule.BaseUrl"/>/
    /// <see cref="ProviderSchedule.AuthType"/>/<see cref="ProviderSchedule.AuthParamName"/>/
    /// <see cref="ProviderSchedule.TimeoutSeconds"/> on an already-existing row, leaving
    /// Enabled/Cron/TimeZone untouched - the one-time catalog migration's backfill path for a
    /// provider that already had a schedule row (possibly with live user edits to its
    /// Enabled/Cron/TimeZone) before those extra fields existed. A no-op if no row exists for this
    /// (Pipeline, Provider) pair.
    /// </summary>
    Task BackfillCatalogFieldsAsync(
        CrawlPipeline pipeline,
        string provider,
        bool saveRawResponses,
        string? baseUrl,
        ApiAuthType? authType,
        string? authParamName,
        int? timeoutSeconds,
        CancellationToken cancellationToken);

    /// <summary>Returns false when no row with that (Pipeline, Provider) pair exists.</summary>
    Task<bool> DeleteAsync(CrawlPipeline pipeline, string provider, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
