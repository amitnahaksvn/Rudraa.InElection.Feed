using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Der Spiegel (spiegel.de, Germany) RSS integration - the English "International" edition at
/// spiegel.de/international/index.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="DerSpiegel"]:Feeds, never hardcoded here.
/// </summary>
public sealed class DerSpiegelRssProvider : BaseRssProvider
{
    public const string ProviderName = "DerSpiegel";
    public const string ClientName = "DerSpiegelRssClient";

    public DerSpiegelRssProvider(IHttpClientFactory httpClientFactory, ILogger<DerSpiegelRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
