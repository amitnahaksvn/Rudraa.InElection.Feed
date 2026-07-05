using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Vietnam News (vietnamnews.vn, Vietnam - English-language) RSS integration - the requested
/// vietnamnews.vn/rss.html 404s; the real feeds are per-section under /rss/{section}.rss (no
/// rel="alternate" tag on the homepage to discover this from - found by testing the site's other
/// known section-feed naming convention), e.g. vietnamnews.vn/rss/homepage.rss for the general
/// feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Vietnam"]:Providers[Name="VietnamNews"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class VietnamNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "VietnamNews";
    public const string ClientName = "VietnamNewsRssClient";

    public VietnamNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<VietnamNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
