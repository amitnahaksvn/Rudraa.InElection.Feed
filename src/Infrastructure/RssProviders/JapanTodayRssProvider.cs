using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Japan Today (japantoday.com, Japan - English-language edition) RSS integration - standard
/// WordPress /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Japan"]:Providers[Name="JapanToday"]:Feeds, never hardcoded here.
/// </summary>
public sealed class JapanTodayRssProvider : BaseRssProvider
{
    public const string ProviderName = "JapanToday";
    public const string ClientName = "JapanTodayRssClient";

    public JapanTodayRssProvider(IHttpClientFactory httpClientFactory, ILogger<JapanTodayRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
