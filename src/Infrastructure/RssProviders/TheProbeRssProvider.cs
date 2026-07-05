using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Probe (theprobe.in, investigative journalism) RSS integration - the working feed is
/// theprobe.in/rss (discovered via the homepage's own rel="alternate" link tag; /feed and
/// /rss.xml both 404). Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TheProbe"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TheProbeRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheProbe";
    public const string ClientName = "TheProbeRssClient";

    public TheProbeRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheProbeRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
