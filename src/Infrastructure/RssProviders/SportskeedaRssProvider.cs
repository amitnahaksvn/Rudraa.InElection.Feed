using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Sportskeeda (sportskeeda.com) RSS integration - only one working feed found: a single
/// all-sports aggregate mixing cricket/football/NFL/WWE/etc. (no section-specific feed could be
/// found, no rel="alternate" RSS link on section pages either). Configured directly against
/// api.sportskeeda.com/v3/feeds_v2/1414 rather than the human-friendly sportskeeda.com/feed alias
/// - that alias 301s via a CloudFront Function (edge compute, not a static redirect rule -
/// visible as "x-cache: FunctionGeneratedResponse from cloudfront" on the 301 itself), which
/// returned inconsistent results between this environment and production (a 405 in production,
/// 301 here) - almost certainly PoP/request-dependent function logic, not a fixed rule. Hitting
/// the resolved API URL directly serves identical content with no redirect involved, sidestepping
/// whatever that function does differently for different callers. Feed URL lives entirely in
/// configuration under NewsCrawler:Providers[Name="Sportskeeda"]:Feeds, never hardcoded here.
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
