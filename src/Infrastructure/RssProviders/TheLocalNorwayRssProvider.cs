using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Local Norway (thelocal.no, Norway - English-language edition) RSS integration - the
/// requested thelocal.no/feed 404s; the real feed is discoverable via the site's own
/// rel="alternate" link tag at feeds.thelocal.com/rss/builder/no (thelocal.com's shared feed
/// builder, keyed by country code - the same platform behind The Local Denmark below). Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Norway"]:Providers[Name="TheLocalNorway"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TheLocalNorwayRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheLocalNorway";
    public const string ClientName = "TheLocalNorwayRssClient";

    public TheLocalNorwayRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheLocalNorwayRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
