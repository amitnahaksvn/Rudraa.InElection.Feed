using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Stuff (stuff.co.nz, New Zealand - English-language) RSS integration - served as Atom 1.0
/// (&lt;feed&gt;/&lt;entry&gt;/&lt;published&gt;), not RSS 2.0, so this extends
/// <see cref="BaseAtomRssProvider"/> rather than <see cref="BaseRssProvider"/> - same reasoning
/// as The Quint/Free Press Journal/National Herald. Unlike those three, Stuff's entries do carry
/// their own media:content image tags, but BaseAtomRssProvider doesn't parse that element (by
/// design, since none of its existing users have one) - falling back to the og:image HTML fetch
/// still works correctly, just with one avoidable extra request per article; accepted as-is
/// rather than special-cased for a single provider. Feed URL lives entirely in configuration
/// under NewsCrawler:Countries[Name="New Zealand"]:Providers[Name="Stuff"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class StuffRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "Stuff";
    public const string ClientName = "StuffRssClient";

    public StuffRssProvider(IHttpClientFactory httpClientFactory, ILogger<StuffRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
