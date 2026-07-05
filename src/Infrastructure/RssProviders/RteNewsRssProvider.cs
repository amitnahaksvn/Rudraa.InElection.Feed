using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RTE News (rte.ie, Ireland - English-language public broadcaster) RSS integration -
/// rte.ie/feeds/rss/?index=/news/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Ireland"]:Providers[Name="RTENews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RteNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "RTENews";
    public const string ClientName = "RteNewsRssClient";

    public RteNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<RteNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
