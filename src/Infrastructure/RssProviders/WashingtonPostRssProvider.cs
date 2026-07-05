using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Washington Post (washingtonpost.com, United States) RSS integration - the classic
/// feeds.washingtonpost.com/rss/{section} endpoints. Feed URLs live entirely in configuration
/// under NewsCrawler:Countries[Name="United States"]:Providers[Name="WashingtonPost"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class WashingtonPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "WashingtonPost";
    public const string ClientName = "WashingtonPostRssClient";

    public WashingtonPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<WashingtonPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
