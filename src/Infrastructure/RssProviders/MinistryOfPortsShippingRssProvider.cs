using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// Ministry of Ports, Shipping and Waterways (shipmin.gov.in) RSS integration - discovered via
/// the site's own &lt;link rel="alternate" type="application/rss+xml"&gt; tag; the only ministry
/// site out of ~50 tried that exposes one. Content skews toward audit/annual-report announcements
/// rather than press-release-style news, and includes one permanent placeholder "Test" item from
/// the publisher's own CMS - both accepted as-is rather than filtered, same as any other
/// publisher's editorial content. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="MinistryOfPortsShipping"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> - production logs show a consistent
/// "Name or service not known (shipmin.gov.in:443)" (a DNS resolution failure specifically from
/// this app's Azure network; the domain resolves and responds fine from a non-Azure network),
/// same edge/network-block category already documented for MPInfo/NDMA, just manifesting as a DNS
/// failure rather than a connection hang or an instant 403.
/// </summary>
public sealed class MinistryOfPortsShippingRssProvider : BaseRssProvider
{
    public const string ProviderName = "MinistryOfPortsShipping";
    public const string ClientName = "MinistryOfPortsShippingRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<MinistryOfPortsShippingRssProvider> _logger;

    public MinistryOfPortsShippingRssProvider(IHttpClientFactory httpClientFactory, ILogger<MinistryOfPortsShippingRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
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
