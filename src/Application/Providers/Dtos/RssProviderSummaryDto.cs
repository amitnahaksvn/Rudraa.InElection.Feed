namespace Application.Providers.Dtos;

public sealed record RssFeedSummaryDto(string Name, string Url, string Category, string Language, bool Enabled);

/// <summary>
/// One RSS provider flattened for the Provider Management page - <see cref="Enabled"/> already
/// folds in the owning country's own flag, since that's what actually determines whether this
/// provider's feeds are ever fetched. <see cref="Enabled"/>/<see cref="Cron"/>/<see cref="TimeZone"/>
/// reflect the live, database-backed <c>ProviderSchedule</c> when one exists for this provider,
/// falling back to <c>NewsCrawler.appsettings.json</c>'s own values only if it doesn't yet.
/// </summary>
public sealed record RssProviderSummaryDto(
    string Country,
    string Name,
    bool Enabled,
    string Cron,
    string TimeZone,
    string Description,
    IReadOnlyList<RssFeedSummaryDto> Feeds);
