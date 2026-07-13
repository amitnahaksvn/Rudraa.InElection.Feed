using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Navhind Times (navhindtimes.in) RSS integration - a Goa-only publisher, standard WordPress
/// /feed/ (discovered via the homepage's own rel="alternate" tag - the page-numbered URL initially
/// tried elsewhere returns an HTML shell, not real content). Feed URL lives entirely in
/// configuration under NewsCrawler:Providers[Name="NavhindTimes"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NavhindTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "NavhindTimes";
    public const string ClientName = "NavhindTimesRssClient";

    public NavhindTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<NavhindTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
