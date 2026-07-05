using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Index.hu (index.hu, Hungary - Hungarian-language) RSS integration - index.hu/24ora/rss/.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Hungary"]:Providers[Name="IndexHu"]:Feeds, never hardcoded here.
/// </summary>
public sealed class IndexHuRssProvider : BaseRssProvider
{
    public const string ProviderName = "IndexHu";
    public const string ClientName = "IndexHuRssClient";

    public IndexHuRssProvider(IHttpClientFactory httpClientFactory, ILogger<IndexHuRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
