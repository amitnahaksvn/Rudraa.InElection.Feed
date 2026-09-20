using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// India Today (indiatoday.in) RSS integration - uses a numeric-id scheme (/rss/{id}), discovered
/// via the site's own /rss index page. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="IndiaToday"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> after 21 unresolved HTTP 403s
/// accumulated in production - same edge/network-block-against-this-app's-Azure-outbound-IP
/// signature already documented for News18/Organiser/IndianExpress/MPInfo/NDMA.
/// </summary>
public sealed class IndiaTodayRssProvider : BaseRssProvider
{
    public const string ProviderName = "IndiaToday";
    public const string ClientName = "IndiaTodayRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly FeedProxyOptions _proxyOptions;
    private readonly ILogger<IndiaTodayRssProvider> _logger;

    public IndiaTodayRssProvider(IHttpClientFactory httpClientFactory, ILogger<IndiaTodayRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions, IOptions<FeedProxyOptions> proxyOptions)
        : base(httpClientFactory, logger)
    {
        _httpClientFactory = httpClientFactory;
        _waybackOptions = waybackOptions.Value;
        _proxyOptions = proxyOptions.Value;
        _logger = logger;
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;

    /// <summary>
    /// Relay first when configured (<see cref="FeedProxyOptions"/> - fresh content, no shared Wayback
    /// quota, and immune to archive.org outages); otherwise straight to the Wayback Machine as before.
    /// </summary>
    protected override Task<string> ResolveFeedUrlAsync(RssFeedOptions feed, CancellationToken cancellationToken) =>
        FeedProxyUrl.Build(_proxyOptions, feed.Url) is { } relayUrl ? Task.FromResult(relayUrl) : ResolveWaybackAsync(feed, cancellationToken);

    /// <summary>Only reached when the relay was tried first and failed.</summary>
    protected override async Task<string?> ResolveFallbackUrlAsync(RssFeedOptions feed, CancellationToken cancellationToken) =>
        FeedProxyUrl.Build(_proxyOptions, feed.Url) is null ? null : await ResolveWaybackAsync(feed, cancellationToken);

    /// <summary>
    /// Relay/Wayback can each time out or be down (archive.org intermittently exceeds the feed
    /// timeout), so the origin URL itself is the last-resort tier - it is only reached once every
    /// earlier tier has failed, so a network that IS blocked pays the extra attempt only on failure.
    /// </summary>
    protected override async Task<IReadOnlyList<string>> ResolveFallbackUrlsAsync(RssFeedOptions feed, CancellationToken cancellationToken)
    {
        var fallbacks = new List<string>();
        if (await ResolveFallbackUrlAsync(feed, cancellationToken) is { } waybackUrl)
        {
            fallbacks.Add(waybackUrl);
        }

        fallbacks.Add(feed.Url);
        return fallbacks;
    }

    private Task<string> ResolveWaybackAsync(RssFeedOptions feed, CancellationToken cancellationToken) =>
        WaybackMachineFeedResolver.ResolveAsync(
            _httpClientFactory.CreateClient(InfrastructureServiceCollectionExtensions.WaybackMachineClientName),
            _waybackOptions,
            feed.Url,
            _logger,
            cancellationToken);
}
