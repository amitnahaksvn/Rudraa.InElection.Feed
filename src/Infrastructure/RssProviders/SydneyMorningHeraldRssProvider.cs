using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Sydney Morning Herald (smh.com.au, Australia) RSS integration - smh.com.au/rss/feed.xml. Feed
/// URL lives entirely in configuration under NewsCrawler:Providers[Name="SydneyMorningHerald"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class SydneyMorningHeraldRssProvider : BaseRssProvider
{
    public const string ProviderName = "SydneyMorningHerald";
    public const string ClientName = "SydneyMorningHeraldRssClient";

    public SydneyMorningHeraldRssProvider(IHttpClientFactory httpClientFactory, ILogger<SydneyMorningHeraldRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
