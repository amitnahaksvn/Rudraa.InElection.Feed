using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// TASS (tass.com, Russia) RSS integration - tass.com/rss/v2.xml. No image tags, relies on the
/// og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TASS"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TassRssProvider : BaseRssProvider
{
    public const string ProviderName = "TASS";
    public const string ClientName = "TassRssClient";

    public TassRssProvider(IHttpClientFactory httpClientFactory, ILogger<TassRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
