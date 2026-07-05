using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Graphic Online (graphic.com.gh, Ghana - English-language) RSS integration - the requested
/// /feed.html 404s; the real feed, declared via rel="alternate" link tags on the homepage, is a
/// Joomla-style query-string feed at /?format=feed&amp;type=rss. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Ghana"]:Providers[Name="GraphicOnline"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class GraphicOnlineRssProvider : BaseRssProvider
{
    public const string ProviderName = "GraphicOnline";
    public const string ClientName = "GraphicOnlineRssClient";

    public GraphicOnlineRssProvider(IHttpClientFactory httpClientFactory, ILogger<GraphicOnlineRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
