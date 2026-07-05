using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Al Jazeera (aljazeera.com, Qatar) RSS integration - aljazeera.com/xml/rss/all.xml. No image
/// tags, relies on the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="AlJazeera"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AlJazeeraRssProvider : BaseRssProvider
{
    public const string ProviderName = "AlJazeera";
    public const string ClientName = "AlJazeeraRssClient";

    public AlJazeeraRssProvider(IHttpClientFactory httpClientFactory, ILogger<AlJazeeraRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
