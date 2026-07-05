using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Bangkok Post (bangkokpost.com, Thailand) RSS integration -
/// bangkokpost.com/rss/data/topstories.xml. No image tags, relies on the og:image HTML fallback.
/// Feed URL lives entirely in configuration under NewsCrawler:Providers[Name="BangkokPost"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class BangkokPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "BangkokPost";
    public const string ClientName = "BangkokPostRssClient";

    public BangkokPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<BangkokPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
