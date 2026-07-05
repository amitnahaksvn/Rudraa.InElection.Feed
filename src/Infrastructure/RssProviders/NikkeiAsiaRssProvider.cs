using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Nikkei Asia (asia.nikkei.com, Japan) RSS integration - an RSS 1.0/RDF feed
/// (asia.nikkei.com/rss/feed/nar). Items carry no date element at all (no pubDate, no dc:date) -
/// PublishedAt is always null for this provider, a genuine feed limitation, not a parsing bug.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Japan"]:Providers[Name="NikkeiAsia"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NikkeiAsiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "NikkeiAsia";
    public const string ClientName = "NikkeiAsiaRssClient";

    public NikkeiAsiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<NikkeiAsiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
