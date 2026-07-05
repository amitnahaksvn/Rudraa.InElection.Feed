using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Irish Times (irishtimes.com, Ireland - English-language) RSS integration - the requested
/// irishtimes.com/feeds/rss/ 404s; the real feed is served through its Arc XP CMS's own
/// outbound-feeds path, irishtimes.com/arc/outboundfeeds/feed-irish-news/?from=0&amp;size=20
/// (discovered by following a working legacy /cmlink/ redirect to its resolved URL). Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Ireland"]:Providers[Name="TheIrishTimes"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TheIrishTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheIrishTimes";
    public const string ClientName = "TheIrishTimesRssClient";

    public TheIrishTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheIrishTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
