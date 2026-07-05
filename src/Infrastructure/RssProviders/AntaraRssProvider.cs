using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Antara (antaranews.com, Indonesia) RSS integration - the English edition at
/// en.antaranews.com/rss/news.xml. No image tags, relies on the og:image HTML fallback. Feed URL
/// lives entirely in configuration under NewsCrawler:Providers[Name="Antara"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class AntaraRssProvider : BaseRssProvider
{
    public const string ProviderName = "Antara";
    public const string ClientName = "AntaraRssClient";

    public AntaraRssProvider(IHttpClientFactory httpClientFactory, ILogger<AntaraRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
