using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Naharnet (naharnet.com, Lebanon - English-language) RSS integration - served as Atom 1.0
/// (&lt;feed&gt;/&lt;entry&gt;/&lt;published&gt;), not RSS 2.0, so this extends
/// <see cref="BaseAtomRssProvider"/> rather than <see cref="BaseRssProvider"/> - same reasoning
/// as The Quint/Stuff. The requested homepage has no bare RSS link; the real feeds are declared
/// via rel="alternate" tags under /tags/{topic}/en/feed.atom - "lebanon" is the general one. No
/// image tags in entries, so images come from the og:image HTML fallback like Stuff. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Lebanon"]:Providers[Name="Naharnet"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NaharnetRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "Naharnet";
    public const string ClientName = "NaharnetRssClient";

    public NaharnetRssProvider(IHttpClientFactory httpClientFactory, ILogger<NaharnetRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
