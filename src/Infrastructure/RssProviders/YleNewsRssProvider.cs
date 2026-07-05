using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// YLE News (yle.fi, Finland - English-language edition) RSS integration -
/// feeds.yle.fi/uutiset/v1/majorHeadlines/YLE_UUTISET.rss. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Finland"]:Providers[Name="YLENews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class YleNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "YLENews";
    public const string ClientName = "YleNewsRssClient";

    public YleNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<YleNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
