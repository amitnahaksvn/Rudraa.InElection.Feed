using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Focus Taiwan (focustaiwan.tw, Taiwan - English-language; the CNA/Central News Agency's own
/// English news service - the user's source table listed "Focus Taiwan" and "Central News
/// Agency" as two separate rows sharing the identical requested URL, and the resolved feed's own
/// &lt;title&gt; confirms they are one and the same publisher, so only one provider is wired up)
/// RSS integration - the requested focustaiwan.tw/rss 404s; the real feed is declared via a
/// rel="alternate" link tag on the homepage, hosted on FeedBurner at
/// feeds.feedburner.com/rsscna/engnews/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Taiwan"]:Providers[Name="FocusTaiwan"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class FocusTaiwanRssProvider : BaseRssProvider
{
    public const string ProviderName = "FocusTaiwan";
    public const string ClientName = "FocusTaiwanRssClient";

    public FocusTaiwanRssProvider(IHttpClientFactory httpClientFactory, ILogger<FocusTaiwanRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
