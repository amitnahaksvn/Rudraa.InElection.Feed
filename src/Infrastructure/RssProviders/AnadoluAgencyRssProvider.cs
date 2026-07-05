using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Anadolu Agency (aa.com.tr, Turkey) RSS integration - the English World feed at
/// aa.com.tr/en/rss/default?cat=world. No image tags, relies on the og:image HTML fallback. Feed
/// URL lives entirely in configuration under NewsCrawler:Providers[Name="AnadoluAgency"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class AnadoluAgencyRssProvider : BaseRssProvider
{
    public const string ProviderName = "AnadoluAgency";
    public const string ClientName = "AnadoluAgencyRssClient";

    public AnadoluAgencyRssProvider(IHttpClientFactory httpClientFactory, ILogger<AnadoluAgencyRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
