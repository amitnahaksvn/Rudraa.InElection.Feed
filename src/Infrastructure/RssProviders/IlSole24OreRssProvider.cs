using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Il Sole 24 Ore (ilsole24ore.com, Italy - business/financial daily) RSS integration -
/// ilsole24ore.com/rss/{section}.xml, discovered via the site's own /rss index page (its
/// section slugs use a "--" separator for subsections, e.g. italia--politica.xml). Feed URLs
/// live entirely in configuration under
/// NewsCrawler:Countries[Name="Italy"]:Providers[Name="IlSole24Ore"]:Feeds, never hardcoded here.
/// </summary>
public sealed class IlSole24OreRssProvider : BaseRssProvider
{
    public const string ProviderName = "IlSole24Ore";
    public const string ClientName = "IlSole24OreRssClient";

    public IlSole24OreRssProvider(IHttpClientFactory httpClientFactory, ILogger<IlSole24OreRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
