using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// El Espectador (elespectador.com, Colombia - Spanish-language) RSS integration - the requested
/// /rss/ 404s; the real feed, declared via a rel="alternate" link tag on the homepage, is Arc
/// XP's own outbound-feeds path,
/// elespectador.com/arc/outboundfeeds/discover/?outputType=xml. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Colombia"]:Providers[Name="ElEspectador"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class ElEspectadorRssProvider : BaseRssProvider
{
    public const string ProviderName = "ElEspectador";
    public const string ClientName = "ElEspectadorRssClient";

    public ElEspectadorRssProvider(IHttpClientFactory httpClientFactory, ILogger<ElEspectadorRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
