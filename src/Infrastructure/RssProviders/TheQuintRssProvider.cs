using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Quint (thequint.com) - served as Atom 1.0 from QuintType's own CDN
/// (prod-qt-images.s3.amazonaws.com/production/thequint/feed.xml), not RSS 2.0 - see
/// <see cref="BaseAtomRssProvider"/>'s own doc comment. Feed URL lives entirely in configuration
/// under NewsCrawler:Providers[Name="TheQuint"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TheQuintRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "TheQuint";
    public const string ClientName = "TheQuintRssClient";

    public TheQuintRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheQuintRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
