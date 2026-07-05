using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// CTK/Ceska tiskova kancelar (ceskenoviny.cz, Czech Republic - Czech-language news agency) RSS
/// integration - the requested ceskenoviny.cz/rss 404s; the real feed, declared via a
/// rel="alternate" link tag on the homepage, is ceskenoviny.cz/sluzby/rss/zpravy.php. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Czech Republic"]:Providers[Name="CTK"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class CtkRssProvider : BaseRssProvider
{
    public const string ProviderName = "CTK";
    public const string ClientName = "CtkRssClient";

    public CtkRssProvider(IHttpClientFactory httpClientFactory, ILogger<CtkRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
