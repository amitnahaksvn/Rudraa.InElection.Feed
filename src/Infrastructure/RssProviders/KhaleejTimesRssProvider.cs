using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Khaleej Times (khaleejtimes.com, UAE - English-language) RSS integration - the requested
/// /rss 404s; the real feeds, declared via rel="alternate" link tags on the homepage, live under
/// khaleejtimes.com/api/v1/collections/{section}.rss - "top-section" is the general-news one.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="UAE"]:Providers[Name="KhaleejTimes"]:Feeds, never hardcoded here.
/// </summary>
public sealed class KhaleejTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "KhaleejTimes";
    public const string ClientName = "KhaleejTimesRssClient";

    public KhaleejTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<KhaleejTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
