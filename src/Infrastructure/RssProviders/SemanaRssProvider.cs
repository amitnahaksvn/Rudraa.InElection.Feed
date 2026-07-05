using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Semana (semana.com, Colombia - Spanish-language) RSS integration - the requested
/// /rss.xml 404s; the real feed is Arc XP's own outbound-feeds path,
/// semana.com/arc/outboundfeeds/rss/ (same CMS/pattern as El Espectador and La Nacion above -
/// Arc XP is Washington Post's publishing platform, licensed out to several Latin American
/// papers). Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Colombia"]:Providers[Name="Semana"]:Feeds, never hardcoded here.
/// </summary>
public sealed class SemanaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Semana";
    public const string ClientName = "SemanaRssClient";

    public SemanaRssProvider(IHttpClientFactory httpClientFactory, ILogger<SemanaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
