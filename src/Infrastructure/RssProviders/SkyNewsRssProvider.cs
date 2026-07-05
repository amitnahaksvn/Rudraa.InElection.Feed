using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Sky News (news.sky.com, United Kingdom) RSS integration - feeds.skynews.com/feeds/rss/home.xml.
/// Feed URL lives entirely in configuration under NewsCrawler:Providers[Name="SkyNews"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class SkyNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "SkyNews";
    public const string ClientName = "SkyNewsRssClient";

    public SkyNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<SkyNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
