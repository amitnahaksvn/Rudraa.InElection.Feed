using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Tehran Times (tehrantimes.com, Iran - English-language) RSS integration -
/// tehrantimes.com/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Iran"]:Providers[Name="TehranTimes"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TehranTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "TehranTimes";
    public const string ClientName = "TehranTimesRssClient";

    public TehranTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<TehranTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
