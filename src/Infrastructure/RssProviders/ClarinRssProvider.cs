using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Clarin (clarin.com, Argentina - Spanish-language) RSS integration -
/// clarin.com/rss/lo-ultimo/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Argentina"]:Providers[Name="Clarin"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ClarinRssProvider : BaseRssProvider
{
    public const string ProviderName = "Clarin";
    public const string ClientName = "ClarinRssClient";

    public ClarinRssProvider(IHttpClientFactory httpClientFactory, ILogger<ClarinRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
