using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RTBF (rtbf.be, Belgium - French-language public broadcaster) RSS integration - the requested
/// rtbf.be/rss 404s and rss.rtbf.be's own root returns 403, but RTBF's own "how RSS works"
/// article (rtbf.be/article/le-flux-rss-mode-d-emploi-3266) lists real per-section feed URLs
/// under that same rss.rtbf.be host; rss.rtbf.be/article/rss/highlight_rtbf_info.xml is the
/// general news one. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Belgium"]:Providers[Name="RTBF"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RtbfRssProvider : BaseRssProvider
{
    public const string ProviderName = "RTBF";
    public const string ClientName = "RtbfRssClient";

    public RtbfRssProvider(IHttpClientFactory httpClientFactory, ILogger<RtbfRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
