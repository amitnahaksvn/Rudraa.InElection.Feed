using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Peru Reports (perureports.com, Peru - English-language) RSS integration - standard WordPress
/// /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Peru"]:Providers[Name="PeruReports"]:Feeds, never hardcoded here.
/// </summary>
public sealed class PeruReportsRssProvider : BaseRssProvider
{
    public const string ProviderName = "PeruReports";
    public const string ClientName = "PeruReportsRssClient";

    public PeruReportsRssProvider(IHttpClientFactory httpClientFactory, ILogger<PeruReportsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
