using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Ministry of Ports, Shipping and Waterways (shipmin.gov.in) RSS integration - discovered via
/// the site's own &lt;link rel="alternate" type="application/rss+xml"&gt; tag; the only ministry
/// site out of ~50 tried that exposes one. Content skews toward audit/annual-report announcements
/// rather than press-release-style news, and includes one permanent placeholder "Test" item from
/// the publisher's own CMS - both accepted as-is rather than filtered, same as any other
/// publisher's editorial content. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="MinistryOfPortsShipping"]:Feeds, never hardcoded here.
/// </summary>
public sealed class MinistryOfPortsShippingRssProvider : BaseRssProvider
{
    public const string ProviderName = "MinistryOfPortsShipping";
    public const string ClientName = "MinistryOfPortsShippingRssClient";

    public MinistryOfPortsShippingRssProvider(IHttpClientFactory httpClientFactory, ILogger<MinistryOfPortsShippingRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
