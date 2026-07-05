using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Wall Street Journal (wsj.com, United States) RSS integration - served from
/// feeds.a.dj.com/rss/{Feed}.xml. RSSUSnews.xml returns a bare S3/CloudFront "AccessDenied" XML
/// error body (not real content) and is excluded; RSSWorldNews.xml and RSSMarketsMain.xml both
/// work. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="WSJ"]:Feeds, never hardcoded here.
/// </summary>
public sealed class WsjRssProvider : BaseRssProvider
{
    public const string ProviderName = "WSJ";
    public const string ClientName = "WsjRssClient";

    public WsjRssProvider(IHttpClientFactory httpClientFactory, ILogger<WsjRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
