using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Frontline (frontline.thehindu.com, The Hindu Group's fortnightly magazine) RSS integration -
/// same {section}/feeder/default.rss pattern as TheHindu itself. Feed URLs live entirely in
/// configuration under NewsCrawler:Providers[Name="Frontline"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FrontlineRssProvider : BaseRssProvider
{
    public const string ProviderName = "Frontline";
    public const string ClientName = "FrontlineRssClient";

    public FrontlineRssProvider(IHttpClientFactory httpClientFactory, ILogger<FrontlineRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
