using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Japan Times (japantimes.co.jp, Japan) RSS integration - standard WordPress /feed/. Feed
/// URL lives entirely in configuration under NewsCrawler:Providers[Name="JapanTimes"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class JapanTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "JapanTimes";
    public const string ClientName = "JapanTimesRssClient";

    public JapanTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<JapanTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
