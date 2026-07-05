using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// MarketWatch (marketwatch.com, United States) RSS integration - the classic
/// feeds.marketwatch.com/marketwatch/topstories endpoint. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="MarketWatch"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class MarketWatchRssProvider : BaseRssProvider
{
    public const string ProviderName = "MarketWatch";
    public const string ClientName = "MarketWatchRssClient";

    public MarketWatchRssProvider(IHttpClientFactory httpClientFactory, ILogger<MarketWatchRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
