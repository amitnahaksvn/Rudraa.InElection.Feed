using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Telangana Today (telanganatoday.com) RSS integration - only the bare /feed works; every
/// category-specific guess (telangana/feed, category/andhra-pradesh/feed, opinion/feed) silently
/// aliases back to the exact same 500-item everything-feed rather than actually filtering, and
/// national-international/feed returns 0 items - so unlike ZeeNews/TheWeek/IndiaToday's "narrow
/// feeds over noisy aggregate" pattern, this one large aggregate is the only real option and is
/// wired up as-is (same situation as DeccanChronicle/DeccanHerald/NewIndianExpress, each of which
/// also has exactly one working feed). Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TelanganaToday"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TelanganaTodayRssProvider : BaseRssProvider
{
    public const string ProviderName = "TelanganaToday";
    public const string ClientName = "TelanganaTodayRssClient";

    public TelanganaTodayRssProvider(IHttpClientFactory httpClientFactory, ILogger<TelanganaTodayRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
