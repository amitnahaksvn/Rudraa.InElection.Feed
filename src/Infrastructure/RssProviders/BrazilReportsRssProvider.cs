using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Brazil Reports (brazilreports.com, Brazil - English-language edition) RSS integration -
/// standard WordPress /feed. A genuinely low-frequency-publishing site (newest item ~5-6 weeks
/// old at verification time) rather than a broken/frozen feed - kept as configured, same
/// "keep it, note the low volume" precedent as IndiaTV/Telegraph-News-Politics/PoliticsCoUk
/// elsewhere in this codebase. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Brazil"]:Providers[Name="BrazilReports"]:Feeds, never hardcoded here.
/// </summary>
public sealed class BrazilReportsRssProvider : BaseRssProvider
{
    public const string ProviderName = "BrazilReports";
    public const string ClientName = "BrazilReportsRssClient";

    public BrazilReportsRssProvider(IHttpClientFactory httpClientFactory, ILogger<BrazilReportsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
