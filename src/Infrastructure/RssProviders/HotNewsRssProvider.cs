using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// HotNews (hotnews.ro, Romania - Romanian-language) RSS integration - hotnews.ro/rss.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Romania"]:Providers[Name="HotNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class HotNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "HotNews";
    public const string ClientName = "HotNewsRssClient";

    public HotNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<HotNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
