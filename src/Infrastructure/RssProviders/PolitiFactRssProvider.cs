using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// PolitiFact (politifact.com, USA - English-language political fact-checking) RSS integration -
/// the bare /feed and /rss both 404; the real feed, declared via a rel="alternate" link tag on
/// the homepage, is politifact.com/rss/factchecks/. Feed URL lives entirely in configuration
/// under NewsCrawler:Countries[Name="United States"]:Providers[Name="PolitiFact"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class PolitiFactRssProvider : BaseRssProvider
{
    public const string ProviderName = "PolitiFact";
    public const string ClientName = "PolitiFactRssClient";

    public PolitiFactRssProvider(IHttpClientFactory httpClientFactory, ILogger<PolitiFactRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
