using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Taipei Times (taipeitimes.com, Taiwan - English-language) RSS integration - the requested
/// /rss/rss.xml 404s; the real feed, declared via a rel="alternate" link tag on the homepage, is
/// taipeitimes.com/xml/index.rss - an RSS 1.0/RDF feed using dc:date instead of pubDate, already
/// covered by <see cref="BaseRssProvider"/>'s existing Dublin Core date fallback, no code change
/// needed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Taiwan"]:Providers[Name="TaipeiTimes"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TaipeiTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "TaipeiTimes";
    public const string ClientName = "TaipeiTimesRssClient";

    public TaipeiTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<TaipeiTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
