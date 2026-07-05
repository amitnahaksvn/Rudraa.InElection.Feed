using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Folha de S.Paulo (folha.uol.com.br, Brazil - Portuguese-language edition) RSS integration -
/// feeds.folha.uol.com.br/emcimadahora/rss091.xml. An RSS 0.91 feed (not 2.0, and with no
/// pubDate at all - PublishedAt is always null for this provider, same situation as Nikkei Asia
/// elsewhere in this file), but still plain &lt;item&gt; elements with no namespaces, so no
/// BaseRssProvider parsing changes are needed. One known caveat, not fixed: the feed declares
/// `encoding="ISO-8859-1"` in its own XML prolog but the HTTP response's Content-Type header
/// omits a charset, so .NET's default UTF-8 decode of the raw bytes could mangle accented
/// Portuguese characters (title/description text) - links, dedup, and persistence are unaffected
/// either way since those fields are ASCII-safe. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Brazil"]:Providers[Name="Folha"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FolhaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Folha";
    public const string ClientName = "FolhaRssClient";

    public FolhaRssProvider(IHttpClientFactory httpClientFactory, ILogger<FolhaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
