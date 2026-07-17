namespace Application.Providers.Dtos;

/// <summary><see cref="Url"/> is the provider's <c>BaseUrl</c> joined with this endpoint's own <c>Endpoint</c> path - the actual URL a fetch hits, computed once here rather than making every consumer join <see cref="ApiProviderSummaryDto.BaseUrl"/> and <see cref="Endpoint"/> itself.</summary>
public sealed record ApiEndpointSummaryDto(string Name, string Endpoint, string Url, string Category, string Language, bool Enabled);

/// <summary>
/// One JSON news-API provider flattened for the Provider Management page - <see cref="Enabled"/>
/// already folds in the owning country's own flag, same reasoning as
/// <see cref="RssProviderSummaryDto"/>. <see cref="Enabled"/>/<see cref="Cron"/>/
/// <see cref="TimeZone"/> reflect the live, database-backed <c>ProviderSchedule</c> the same way.
/// </summary>
public sealed record ApiProviderSummaryDto(
    string Country,
    string Name,
    bool Enabled,
    string Cron,
    string TimeZone,
    string BaseUrl,
    string AuthType,
    string Description,
    IReadOnlyList<ApiEndpointSummaryDto> Endpoints);
