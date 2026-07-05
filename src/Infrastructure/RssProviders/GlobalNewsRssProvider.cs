using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Global News (globalnews.ca, Canada) RSS integration - standard WordPress /feed/. Feed URL
/// lives entirely in configuration under NewsCrawler:Providers[Name="GlobalNews"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class GlobalNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "GlobalNews";
    public const string ClientName = "GlobalNewsRssClient";

    public GlobalNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<GlobalNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
