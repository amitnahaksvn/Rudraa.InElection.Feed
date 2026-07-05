using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// UN News (news.un.org, English-language) RSS integration -
/// news.un.org/feed/subscribe/en/news/all/rss.xml. Not tied to any single nation, so it's wired
/// under a new "International" pseudo-country (alongside Snopes below) rather than forced under
/// one - the same one-flag-disables-the-group benefit the Countries hierarchy gives every real
/// country. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="International"]:Providers[Name="UNNews"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class UnNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "UNNews";
    public const string ClientName = "UnNewsRssClient";

    public UnNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<UnNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
