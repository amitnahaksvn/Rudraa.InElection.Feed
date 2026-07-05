using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Roya News (royanews.tv, Jordan - Arabic-language) RSS integration - served as Atom 1.0
/// (&lt;feed&gt;/&lt;entry&gt;/&lt;published&gt;), not RSS 2.0, so this extends
/// <see cref="BaseAtomRssProvider"/> rather than <see cref="BaseRssProvider"/> - same reasoning
/// as Naharnet/Stuff/The Quint. No image tags in entries, so images come from the og:image HTML
/// fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Jordan"]:Providers[Name="RoyaNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RoyaNewsRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "RoyaNews";
    public const string ClientName = "RoyaNewsRssClient";

    public RoyaNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<RoyaNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
