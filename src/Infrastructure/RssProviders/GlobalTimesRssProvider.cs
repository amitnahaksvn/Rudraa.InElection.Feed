using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Global Times (globaltimes.cn, China - English-language, state-affiliated edition) RSS
/// integration - globaltimes.cn/rss/outbrain.xml. Unlike Xinhua/China Daily (both excluded
/// elsewhere in this file for serving years-stale content despite returning HTTP 200), this feed
/// is genuinely live and current. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="China"]:Providers[Name="GlobalTimes"]:Feeds, never hardcoded here.
/// </summary>
public sealed class GlobalTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "GlobalTimes";
    public const string ClientName = "GlobalTimesRssClient";

    public GlobalTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<GlobalTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
