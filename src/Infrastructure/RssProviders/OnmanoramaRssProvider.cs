using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Onmanorama (onmanorama.com, Kerala's Manorama Group English edition) RSS integration - single
/// working feed at onmanorama.com/news.feeds.rss.xml. Feed URL lives entirely in configuration
/// under NewsCrawler:Providers[Name="Onmanorama"]:Feeds, never hardcoded here.
/// </summary>
public sealed class OnmanoramaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Onmanorama";
    public const string ClientName = "OnmanoramaRssClient";

    public OnmanoramaRssProvider(IHttpClientFactory httpClientFactory, ILogger<OnmanoramaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
