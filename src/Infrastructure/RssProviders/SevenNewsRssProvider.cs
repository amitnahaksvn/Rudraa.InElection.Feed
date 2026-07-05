using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// 7NEWS (7news.com.au, Australia) RSS integration - 7news.com.au/rss. Feed URL lives entirely in
/// configuration under NewsCrawler:Countries[Name="Australia"]:Providers[Name="SevenNews"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class SevenNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "SevenNews";
    public const string ClientName = "SevenNewsRssClient";

    public SevenNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<SevenNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
