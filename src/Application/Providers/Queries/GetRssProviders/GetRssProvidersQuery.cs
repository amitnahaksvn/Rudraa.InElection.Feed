using Mediator;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Options;
using Application.Providers.Dtos;
using Domain.Enums;

namespace Application.Providers.Queries.GetRssProviders;

/// <summary>Every configured RSS provider (across every country), flattened for the Provider Management page's "RSS Feeds" tab. Enabled/Cron/TimeZone come from the live, database-backed <c>ProviderSchedule</c> when one exists, falling back to <c>NewsCrawler.appsettings.json</c> otherwise.</summary>
public sealed record GetRssProvidersQuery : IRequest<IReadOnlyList<RssProviderSummaryDto>>;

public sealed class GetRssProvidersQueryHandler : IRequestHandler<GetRssProvidersQuery, IReadOnlyList<RssProviderSummaryDto>>
{
    private readonly IOptions<NewsCrawlerOptions> _options;
    private readonly IProviderScheduleRepository _schedules;

    public GetRssProvidersQueryHandler(IOptions<NewsCrawlerOptions> options, IProviderScheduleRepository schedules)
    {
        _options = options;
        _schedules = schedules;
    }

    public async ValueTask<IReadOnlyList<RssProviderSummaryDto>> Handle(GetRssProvidersQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _schedules.GetAllAsync(CrawlPipeline.Rss, cancellationToken);
        var scheduleByProvider = schedules.ToDictionary(s => s.Provider, StringComparer.OrdinalIgnoreCase);

        return _options.Value.Countries
            .SelectMany(country => country.Providers.Select(provider =>
            {
                scheduleByProvider.TryGetValue(provider.Name, out var schedule);
                return new RssProviderSummaryDto(
                    country.Name,
                    provider.Name,
                    country.Enabled && (schedule?.Enabled ?? provider.Enabled),
                    schedule?.Cron ?? provider.Cron,
                    schedule?.TimeZone ?? "UTC",
                    BuildDescription(provider),
                    provider.Feeds.Select(f => new RssFeedSummaryDto(f.Name, f.Url, f.Category, f.Language, f.Enabled)).ToList());
            }))
            .ToList();
    }

    // No free-text description is stored per provider in configuration (writing one by hand for
    // every one of the 200+ RSS providers wouldn't stay maintained) - this is computed instead
    // from what's already there, so it can never drift out of sync with the actual feed list.
    private static string BuildDescription(RssProviderOptions provider)
    {
        var enabledCount = provider.Feeds.Count(f => f.Enabled);
        var categories = provider.Feeds
            .Select(f => f.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .Take(4)
            .ToList();

        var categoryText = categories.Count > 0 ? $" covering {string.Join(", ", categories)}" : string.Empty;
        var feedWord = provider.Feeds.Count == 1 ? "feed" : "feeds";
        return $"{enabledCount} of {provider.Feeds.Count} {feedWord} enabled{categoryText}.";
    }
}
