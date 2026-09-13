using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// News18 (news18.com) RSS integration. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="News18"]:Feeds, never hardcoded here.
///
/// Two distinct problems accumulated 668+ unresolved HTTP 403s in production before this was
/// re-diagnosed on 2026-09-13:
/// (1) Every configured feed URL (<c>news18.com/rss/{slug}.xml</c>) now permanently 308-redirects
/// to <c>news18.com/commonfeeds/v1/eng/rss/{slug}.xml</c> - config updated to the new canonical
/// URLs directly (one slug, "lifestyle", also renamed to "lifestyle-2" server-side).
/// (2) News18's Akamai CDN was switched to a browser-style UA under the belief this was the same
/// edge/IP block as IndianExpressRssProvider/MPInfoRssProvider/NdmaRssProvider (this provider
/// used to route through WaybackMachineFeedResolver as a result). Verified via curl from a GitHub
/// Actions runner (an IP range with no history of being blocked here) that this was never an IP
/// problem: a declared-browser UA gets 403'd from that unrelated IP too, and so does no UA at
/// all, while a plain, honestly-generic HTTP-client UA passes cleanly - see
/// <see cref="Infrastructure.DependencyInjection.InfrastructureServiceCollectionExtensions"/>'s
/// <c>GenericHttpClientUserAgent</c>. Akamai here blocks UAs that *claim* to be a browser, not UAs
/// that identify as automation - the opposite problem shape from every other Akamai-blocked
/// provider in this codebase, so no Wayback Machine indirection is needed once the UA is right.
/// </summary>
public sealed class News18RssProvider : BaseRssProvider
{
    public const string ProviderName = "News18";
    public const string ClientName = "News18RssClient";

    public News18RssProvider(IHttpClientFactory httpClientFactory, ILogger<News18RssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
