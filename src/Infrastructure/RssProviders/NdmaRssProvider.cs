using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// National Disaster Management Authority (ndma.gov.in) RSS integration - the bare /rss.xml
/// endpoint. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="NDMA"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NdmaRssProvider : BaseRssProvider
{
    public const string ProviderName = "NDMA";
    public const string ClientName = "NdmaRssClient";

    public NdmaRssProvider(IHttpClientFactory httpClientFactory, ILogger<NdmaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
