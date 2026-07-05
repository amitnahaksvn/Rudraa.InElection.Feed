using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// PBS NewsHour (pbs.org/newshour, United States) RSS integration - the working feed is
/// pbs.org/newshour/feeds/rss/headlines; the bare pbs.org/newshour/feeds/rss returns HTTP 202
/// with an empty body, not real content. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="PBSNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class PbsNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "PBSNews";
    public const string ClientName = "PbsNewsRssClient";

    public PbsNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<PbsNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
