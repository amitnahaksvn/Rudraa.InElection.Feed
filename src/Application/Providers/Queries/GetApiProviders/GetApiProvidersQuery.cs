using Mediator;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Options;
using Application.Providers.Dtos;
using Domain.Enums;

namespace Application.Providers.Queries.GetApiProviders;

/// <summary>Every configured JSON news-API provider (across every country), flattened for the Provider Management page's "APIs" tab. Enabled/Cron/TimeZone come from the live, database-backed <c>ProviderSchedule</c> when one exists, falling back to <c>NewsApiCrawler</c> config otherwise.</summary>
public sealed record GetApiProvidersQuery : IRequest<IReadOnlyList<ApiProviderSummaryDto>>;

public sealed class GetApiProvidersQueryHandler : IRequestHandler<GetApiProvidersQuery, IReadOnlyList<ApiProviderSummaryDto>>
{
    private readonly IOptions<NewsApiCrawlerOptions> _options;
    private readonly IProviderScheduleRepository _schedules;

    public GetApiProvidersQueryHandler(IOptions<NewsApiCrawlerOptions> options, IProviderScheduleRepository schedules)
    {
        _options = options;
        _schedules = schedules;
    }

    public async ValueTask<IReadOnlyList<ApiProviderSummaryDto>> Handle(GetApiProvidersQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _schedules.GetAllAsync(CrawlPipeline.Api, cancellationToken);
        var scheduleByProvider = schedules.ToDictionary(s => s.Provider, StringComparer.OrdinalIgnoreCase);

        return _options.Value.Countries
            .SelectMany(country => country.Providers.Select(provider =>
            {
                scheduleByProvider.TryGetValue(provider.Name, out var schedule);
                return new ApiProviderSummaryDto(
                    country.Name,
                    provider.Name,
                    country.Enabled && (schedule?.Enabled ?? provider.Enabled),
                    schedule?.Cron ?? provider.Cron,
                    schedule?.TimeZone ?? "UTC",
                    provider.BaseUrl,
                    provider.AuthType.ToString(),
                    BuildDescription(provider),
                    provider.Endpoints
                        .Select(e => new ApiEndpointSummaryDto(e.Name, e.Endpoint, BuildEndpointUrl(provider.BaseUrl, e.Endpoint), e.Category, e.Language, e.Enabled))
                        .ToList());
            }))
            .ToList();
    }

    // Same join `BaseNewsApiProvider.BuildRequestUrl` uses at fetch time (Infrastructure isn't
    // referenceable from here, so this mirrors it rather than reusing it) - minus query
    // parameters/auth, since this is display-only and must never risk leaking an API key.
    // EventRegistryProvider's one endpoint configures an empty Endpoint (it's POST-body driven,
    // not path-driven - see its own doc comment) and just uses BaseUrl as-is; an empty path here
    // falls through to that same bare-BaseUrl result rather than appending a trailing slash.
    private static string BuildEndpointUrl(string baseUrl, string endpoint)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var trimmedPath = endpoint.Trim('/');
        return trimmedPath.Length == 0 ? trimmedBase : $"{trimmedBase}/{trimmedPath}";
    }

    // Same reasoning as GetRssProvidersQueryHandler's own BuildDescription - computed from the
    // endpoint list rather than hand-written per provider, so it can't go stale.
    private static string BuildDescription(NewsApiProviderOptions provider)
    {
        var enabledCount = provider.Endpoints.Count(e => e.Enabled);
        var categories = provider.Endpoints
            .Select(e => e.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .Take(4)
            .ToList();

        var categoryText = categories.Count > 0 ? $" covering {string.Join(", ", categories)}" : string.Empty;
        var endpointWord = provider.Endpoints.Count == 1 ? "endpoint" : "endpoints";
        return $"{enabledCount} of {provider.Endpoints.Count} {endpointWord} enabled{categoryText}.";
    }
}
