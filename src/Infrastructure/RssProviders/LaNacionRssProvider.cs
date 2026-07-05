using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// La Nacion (lanacion.com.ar, Argentina - Spanish-language) RSS integration - Arc XP's own
/// outbound-feeds path, lanacion.com.ar/arc/outboundfeeds/rss/. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Argentina"]:Providers[Name="LaNacion"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class LaNacionRssProvider : BaseRssProvider
{
    public const string ProviderName = "LaNacion";
    public const string ClientName = "LaNacionRssClient";

    public LaNacionRssProvider(IHttpClientFactory httpClientFactory, ILogger<LaNacionRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
