using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Republica (myrepublica.nagariknetwork.com, Nepal - English-language) RSS integration - the
/// requested /rss 404s; the real feed, declared via a rel="alternate" link tag on the homepage,
/// is myrepublica.nagariknetwork.com/feeds. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Nepal"]:Providers[Name="Republica"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RepublicaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Republica";
    public const string ClientName = "RepublicaRssClient";

    public RepublicaRssProvider(IHttpClientFactory httpClientFactory, ILogger<RepublicaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
