using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Local Sweden (thelocal.se, Sweden - English-language edition) RSS integration - the
/// originally-requested thelocal.se/feed 404s and was marked dead in an earlier pass, but
/// thelocal.com's shared feed-builder platform (discovered via The Local Norway/Denmark in a
/// later batch) also covers Sweden at feeds.thelocal.com/rss/builder/se, so this was added
/// retroactively once that pattern was found. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Sweden"]:Providers[Name="TheLocalSweden"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TheLocalSwedenRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheLocalSweden";
    public const string ClientName = "TheLocalSwedenRssClient";

    public TheLocalSwedenRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheLocalSwedenRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
