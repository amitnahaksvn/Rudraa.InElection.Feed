using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// National Herald (nationalheraldindia.com) - served as Atom 1.0 from QuintType's own CDN
/// (prod-qt-images.s3.amazonaws.com/production/nationalherald/feed.xml), not RSS 2.0 - see
/// <see cref="BaseAtomRssProvider"/>'s own doc comment. Feed URL lives entirely in configuration
/// under NewsCrawler:Providers[Name="NationalHerald"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NationalHeraldRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "NationalHerald";
    public const string ClientName = "NationalHeraldRssClient";

    public NationalHeraldRssProvider(IHttpClientFactory httpClientFactory, ILogger<NationalHeraldRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
