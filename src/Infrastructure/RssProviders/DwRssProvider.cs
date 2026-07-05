using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// DW / Deutsche Welle (dw.com, Germany) RSS integration - rss.dw.com/xml/rss-en-all. No image
/// tags, relies on the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="DW"]:Feeds, never hardcoded here.
/// </summary>
public sealed class DwRssProvider : BaseRssProvider
{
    public const string ProviderName = "DW";
    public const string ClientName = "DwRssClient";

    public DwRssProvider(IHttpClientFactory httpClientFactory, ILogger<DwRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
