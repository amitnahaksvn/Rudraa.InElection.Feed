using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// DutchNews (dutchnews.nl, Netherlands - English-language edition) RSS integration - standard
/// WordPress /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Netherlands"]:Providers[Name="DutchNews"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class DutchNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "DutchNews";
    public const string ClientName = "DutchNewsRssClient";

    public DutchNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<DutchNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
