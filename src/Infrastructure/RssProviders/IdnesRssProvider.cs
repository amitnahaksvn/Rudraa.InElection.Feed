using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// iDNES (idnes.cz, Czech Republic - Czech-language) RSS integration -
/// servis.idnes.cz/rss.aspx. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Czech Republic"]:Providers[Name="iDNES"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class IdnesRssProvider : BaseRssProvider
{
    public const string ProviderName = "iDNES";
    public const string ClientName = "IdnesRssClient";

    public IdnesRssProvider(IHttpClientFactory httpClientFactory, ILogger<IdnesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
