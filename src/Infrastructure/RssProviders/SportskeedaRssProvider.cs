using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Sportskeeda (sportskeeda.com) RSS integration - only one working feed found: the bare /feed
/// endpoint, which redirects to api.sportskeeda.com/v3/feeds_v2/1414 (a single all-sports
/// aggregate mixing cricket/football/NFL/WWE/etc. - no section-specific feed could be found, no
/// rel="alternate" RSS link on section pages either). Feed URL lives entirely in configuration
/// under NewsCrawler:Providers[Name="Sportskeeda"]:Feeds, never hardcoded here.
/// </summary>
public sealed class SportskeedaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Sportskeeda";
    public const string ClientName = "SportskeedaRssClient";

    public SportskeedaRssProvider(IHttpClientFactory httpClientFactory, ILogger<SportskeedaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
