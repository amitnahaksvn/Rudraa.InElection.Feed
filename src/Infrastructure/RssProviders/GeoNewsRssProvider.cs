using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Geo News (geo.tv, Pakistan - English-language) RSS integration - geo.tv/rss/1/1.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Pakistan"]:Providers[Name="GeoNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class GeoNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "GeoNews";
    public const string ClientName = "GeoNewsRssClient";

    public GeoNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<GeoNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
