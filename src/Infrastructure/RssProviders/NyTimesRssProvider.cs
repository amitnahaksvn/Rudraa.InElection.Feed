using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The New York Times (nytimes.com) RSS integration - feeds are served from the legacy
/// rss.nytimes.com/services/xml/rss/nyt/{Section}.xml pattern (e.g. World.xml, Business.xml).
/// The Sports.xml feed returns 0 items and is excluded. Feed URLs live entirely in configuration
/// under NewsCrawler:Providers[Name="NYTimes"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NyTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "NYTimes";
    public const string ClientName = "NyTimesRssClient";

    public NyTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<NyTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
