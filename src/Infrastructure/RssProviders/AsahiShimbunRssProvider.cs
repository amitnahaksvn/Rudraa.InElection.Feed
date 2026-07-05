using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Asahi Shimbun (asahi.com, Japan - Japanese-language edition) RSS integration - an RSS
/// 1.0/RDF feed (asahi.com/rss/asahi/newsheadlines.rdf) using Dublin Core's &lt;dc:date&gt;
/// instead of &lt;pubDate&gt; - handled centrally in BaseRssProvider.ParseItemAsync's date
/// lookup (falls back to dc:date only when no pubDate-named element exists), not an Asahi-only
/// override, since dc:date is a general RSS 1.0/RDF convention other feeds could plausibly use
/// too. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Japan"]:Providers[Name="AsahiShimbun"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AsahiShimbunRssProvider : BaseRssProvider
{
    public const string ProviderName = "AsahiShimbun";
    public const string ClientName = "AsahiShimbunRssClient";

    public AsahiShimbunRssProvider(IHttpClientFactory httpClientFactory, ILogger<AsahiShimbunRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
