using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Guardian (theguardian.com, United Kingdom) RSS integration - theguardian.com/world/rss.
/// Distinct from the JSON News-API "Guardian" provider (Infrastructure/NewsApiProviders,
/// INewsApiProvider, that pipeline currently disabled) - this one is a plain RSS 2.0 feed, no API
/// key needed. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TheGuardian"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TheGuardianRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheGuardian";
    public const string ClientName = "TheGuardianRssClient";

    public TheGuardianRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheGuardianRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
