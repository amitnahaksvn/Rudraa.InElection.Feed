using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// CBC News (cbc.ca, Canada) RSS integration - cbc.ca/webfeed/rss/rss-topstories. Items carry a
/// nonstandard "EDT"/"EST" zone abbreviation on pubDate (e.g. "Wed, 24 Jun 2026 21:33:43 EDT") -
/// handled centrally in BaseRssProvider.ParsePublishDate's North-American-timezone tier, not a
/// CBC-only override. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="CBCNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class CbcNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "CBCNews";
    public const string ClientName = "CbcNewsRssClient";

    public CbcNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<CbcNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
