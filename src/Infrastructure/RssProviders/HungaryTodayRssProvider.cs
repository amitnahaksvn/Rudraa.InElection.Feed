using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Hungary Today (hungarytoday.hu, Hungary - English-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Hungary"]:Providers[Name="HungaryToday"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class HungaryTodayRssProvider : BaseRssProvider
{
    public const string ProviderName = "HungaryToday";
    public const string ClientName = "HungaryTodayRssClient";

    public HungaryTodayRssProvider(IHttpClientFactory httpClientFactory, ILogger<HungaryTodayRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
