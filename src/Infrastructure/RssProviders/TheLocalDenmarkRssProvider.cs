using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Local Denmark (thelocal.dk, Denmark - English-language edition) RSS integration - the
/// requested thelocal.dk/feed 404s; the real feed is thelocal.com's shared feed builder keyed by
/// country code, feeds.thelocal.com/rss/builder/dk - same platform as The Local Norway above.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Denmark"]:Providers[Name="TheLocalDenmark"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class TheLocalDenmarkRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheLocalDenmark";
    public const string ClientName = "TheLocalDenmarkRssClient";

    public TheLocalDenmarkRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheLocalDenmarkRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
