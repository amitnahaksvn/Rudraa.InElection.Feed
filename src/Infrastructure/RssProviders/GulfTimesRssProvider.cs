using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Gulf Times (gulf-times.com, Qatar) RSS integration - the bare `/rss` path is only an HTML
/// index page listing numbered feed links (`/rssFeed/{id}`), not a feed itself; `/rssFeed/9`
/// (the first one listed) is a real, working, frequently-updated feed and is what's configured
/// here. The channel `<title>` is just "Gulf Times" for every numbered id with no category
/// distinction discoverable, so this is treated as the single general feed, same pattern as
/// other single-feed-only providers elsewhere in this file. Feed URL lives entirely in
/// configuration under NewsCrawler:Countries[Name="Qatar"]:Providers[Name="GulfTimes"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class GulfTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "GulfTimes";
    public const string ClientName = "GulfTimesRssClient";

    public GulfTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<GulfTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
