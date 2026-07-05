using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Mainichi Japan (mainichi.jp/english, Japan - The Mainichi's English edition) RSS integration -
/// mainichi.jp/english/rss/etc/mainichi-flash.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Japan"]:Providers[Name="MainichiJapan"]:Feeds, never hardcoded here.
/// </summary>
public sealed class MainichiJapanRssProvider : BaseRssProvider
{
    public const string ProviderName = "MainichiJapan";
    public const string ClientName = "MainichiJapanRssClient";

    public MainichiJapanRssProvider(IHttpClientFactory httpClientFactory, ILogger<MainichiJapanRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
