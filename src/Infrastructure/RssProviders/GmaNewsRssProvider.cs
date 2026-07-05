using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// GMA News (gmanetwork.com, Philippines - English-language) RSS integration - the requested
/// gmanetwork.com/news/rss/news 404s; the real feeds live on a separate data subdomain using
/// GMA's own legacy per-section scheme, data.gmanetwork.com/gno/rss/news/{section}/feed.xml
/// (found via a rel="alternate" link tag on the /news/rss/ index page) - "nation" is the
/// general-news section. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Philippines"]:Providers[Name="GMANews"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class GmaNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "GMANews";
    public const string ClientName = "GmaNewsRssClient";

    public GmaNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<GmaNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
