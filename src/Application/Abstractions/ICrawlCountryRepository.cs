using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

/// <summary>Persistence for <see cref="CrawlCountry"/> - the database-backed replacement for the old JSON country-level Enabled flag.</summary>
public interface ICrawlCountryRepository
{
    Task<IReadOnlyList<CrawlCountry>> GetAllAsync(CrawlPipeline pipeline, CancellationToken cancellationToken);

    Task<CrawlCountry?> GetByNameAsync(CrawlPipeline pipeline, string name, CancellationToken cancellationToken);

    /// <summary>Inserts a document with these values only if one doesn't already exist for this (Pipeline, Name) pair - the one-time migration bootstrap from the legacy JSON files.</summary>
    Task SeedIfMissingAsync(CrawlCountry country, CancellationToken cancellationToken);

    /// <summary>Creates a brand-new country or fully overwrites an existing one - the user-driven add/edit path from the Provider Management page.</summary>
    Task UpsertAsync(CrawlCountry country, CancellationToken cancellationToken);

    /// <summary>Returns false when no row with that (Pipeline, Name) pair exists.</summary>
    Task<bool> DeleteAsync(CrawlPipeline pipeline, string name, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
