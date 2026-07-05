using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Bloomberg (bloomberg.com, United States) RSS integration - the classic feeds.bloomberg.com/
/// {section}/news.rss endpoints. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="Bloomberg"]:Feeds, never hardcoded here.
/// </summary>
public sealed class BloombergRssProvider : BaseRssProvider
{
    public const string ProviderName = "Bloomberg";
    public const string ClientName = "BloombergRssClient";

    public BloombergRssProvider(IHttpClientFactory httpClientFactory, ILogger<BloombergRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
