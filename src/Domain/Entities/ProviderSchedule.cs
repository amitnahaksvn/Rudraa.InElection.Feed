using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// The live, database-backed schedule for one RSS or JSON-API provider - Enabled/Cron/TimeZone no
/// longer come from <c>NewsCrawler.appsettings.json</c>/<c>NewsApiCrawler</c> once a document
/// exists here for a given (Pipeline, Provider) pair. Seeded once per provider from that file's
/// then-current values (see <c>Infrastructure.Seed.ProviderScheduleSeeder</c>) so nothing changes
/// on migration; editable from then on via the Provider Management page without touching the file
/// or restarting the host. <see cref="Country"/> is display-only (which country's config block the
/// provider was seeded from) - the (Pipeline, Provider) pair is the actual identity.
/// </summary>
public sealed class ProviderSchedule
{
    public string Id { get; set; } = string.Empty;

    public CrawlPipeline Pipeline { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string Cron { get; set; } = string.Empty;

    public string TimeZone { get; set; } = "UTC";

    public DateTimeOffset UpdatedAt { get; set; }
}
