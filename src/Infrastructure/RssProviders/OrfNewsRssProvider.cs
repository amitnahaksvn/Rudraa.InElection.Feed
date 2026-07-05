using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ORF News (orf.at, Austria - German-language public broadcaster) RSS integration - the
/// requested orf.at/stories/rss.xml 404s; the real feed, rss.orf.at/news.xml, is declared via a
/// rel="alternate" link tag on orf.at's own homepage. An RSS 1.0/RDF feed
/// (&lt;channel rdf:about="..."&gt;, dc:date instead of pubDate) - already handled by
/// <see cref="BaseRssProvider"/>'s Dublin Core date fallback, no further changes needed. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Austria"]:Providers[Name="ORFNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class OrfNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "ORFNews";
    public const string ClientName = "OrfNewsRssClient";

    public OrfNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<OrfNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
