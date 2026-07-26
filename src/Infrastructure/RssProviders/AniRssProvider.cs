using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ANI (Asian News International, aninews.in) RSS integration -
/// rss/feed/category/{category}[/{subcategory}].xml pattern, discovered via the site's own
/// /rss-feed/ index page. Standard RSS 2.0, no parsing quirks. Feed URLs live entirely in
/// configuration (CrawlFeed documents), never hardcoded here.
///
/// Every one of the 29 category/subcategory feeds is wired up but disabled by default: each
/// item's own URL carries a frozen October 2024 timestamp (e.g. ".../...20241022145433/"),
/// confirmed unchanged across two independent checks roughly a year apart - the channel's
/// lastBuildDate header is regenerated to the current time on every request regardless, so it is
/// not a reliable liveness signal on its own (same caveat as this app's other frozen-feed finds -
/// CNN/Xinhua/China Daily/Forbes' most-popular feed). Left wired up rather than omitted so
/// re-enabling any of them (e.g. National/Politics) is a pure config flip if ANI ever resumes
/// publishing, without needing this class rewritten.
/// </summary>
public sealed class AniRssProvider : BaseRssProvider
{
    public const string ProviderName = "ANI";
    public const string ClientName = "AniRssClient";

    public AniRssProvider(IHttpClientFactory httpClientFactory, ILogger<AniRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
