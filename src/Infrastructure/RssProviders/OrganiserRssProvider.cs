using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// Organiser Weekly (organiser.org) RSS integration - standard WordPress /feed/. No image tags,
/// relies on the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="Organiser"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> after 329 unresolved HTTP 403s
/// accumulated in production, second only to News18 - same edge/network-block-against-this-app's-
/// Azure-outbound-IP signature already documented for News18/IndianExpress/MPInfo/NDMA, not a UA or
/// content issue.
/// </summary>
public sealed class OrganiserRssProvider : BaseRssProvider
{
    public const string ProviderName = "Organiser";
    public const string ClientName = "OrganiserRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly FeedProxyOptions _proxyOptions;
    private readonly ILogger<OrganiserRssProvider> _logger;

    public OrganiserRssProvider(IHttpClientFactory httpClientFactory, ILogger<OrganiserRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions, IOptions<FeedProxyOptions> proxyOptions)
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

    private Task<string> ResolveWaybackAsync(RssFeedOptions feed, CancellationToken cancellationToken) =>
        WaybackMachineFeedResolver.ResolveAsync(
            _httpClientFactory.CreateClient(InfrastructureServiceCollectionExtensions.WaybackMachineClientName),
            _waybackOptions,
            feed.Url,
            _logger,
            cancellationToken);
}
