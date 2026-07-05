using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Rzeczpospolita (rp.pl, Poland - Polish-language) RSS integration - the requested rp.pl/rss
/// 404s; the real feed, declared via a rel="alternate" link tag on the homepage, is
/// rp.pl/rss_main. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Poland"]:Providers[Name="Rzeczpospolita"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class RzeczpospolitaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Rzeczpospolita";
    public const string ClientName = "RzeczpospolitaRssClient";

    public RzeczpospolitaRssProvider(IHttpClientFactory httpClientFactory, ILogger<RzeczpospolitaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
