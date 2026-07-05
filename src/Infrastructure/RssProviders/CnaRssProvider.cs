using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// CNA / Channel News Asia (channelnewsasia.com, Singapore) RSS integration -
/// channelnewsasia.com/rssfeeds/8395986. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="CNA"]:Feeds, never hardcoded here.
/// </summary>
public sealed class CnaRssProvider : BaseRssProvider
{
    public const string ProviderName = "CNA";
    public const string ClientName = "CnaRssClient";

    public CnaRssProvider(IHttpClientFactory httpClientFactory, ILogger<CnaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
