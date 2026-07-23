using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Database-backed country-level grouping of providers, e.g. "India" or "United States", for one
/// pipeline (Rss or Api) - the sole replacement for the old <c>CountryOptions</c>/
/// <c>NewsApiCountryOptions</c> JSON <c>Enabled</c> flag, now its own first-class, independently
/// toggleable/creatable/deletable record rather than folded invisibly into every provider's
/// computed Enabled. (Pipeline, Name) is this record's identity - <see cref="ProviderSchedule.Country"/>
/// and <see cref="CrawlFeed"/> (indirectly, via their provider's Country) reference it by that name,
/// not by <see cref="Id"/>.
/// </summary>
public sealed class CrawlCountry
{
    public string Id { get; set; } = string.Empty;

    public CrawlPipeline Pipeline { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
}
