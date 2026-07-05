using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Daily Star (thedailystar.net, Bangladesh - English-language) RSS integration -
/// thedailystar.net/frontpage/rss.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Bangladesh"]:Providers[Name="TheDailyStar"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class TheDailyStarRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheDailyStar";
    public const string ClientName = "TheDailyStarRssClient";

    public TheDailyStarRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheDailyStarRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
