using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// BiharNews.com (biharnews.com) RSS integration - a Bihar-only publisher, added for
/// state-level election coverage. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="BiharNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class BiharNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "BiharNews";
    public const string ClientName = "BiharNewsRssClient";

    public BiharNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<BiharNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
