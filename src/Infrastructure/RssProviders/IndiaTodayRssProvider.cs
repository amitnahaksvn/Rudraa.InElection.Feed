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
    private readonly ILogger<IndiaTodayRssProvider> _logger;

    public IndiaTodayRssProvider(IHttpClientFactory httpClientFactory, ILogger<IndiaTodayRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
        : base(httpClientFactory, logger)
    {
        _httpClientFactory = httpClientFactory;
        _waybackOptions = waybackOptions.Value;
        _logger = logger;
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;

    protected override Task<string> ResolveFeedUrlAsync(RssFeedOptions feed, CancellationToken cancellationToken) =>
        WaybackMachineFeedResolver.ResolveAsync(
            _httpClientFactory.CreateClient(InfrastructureServiceCollectionExtensions.WaybackMachineClientName),
            _waybackOptions,
            feed.Url,
            _logger,
            cancellationToken);
}
