using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// BBC News (bbc.co.uk, United Kingdom) RSS integration - the classic feeds.bbci.co.uk/news/...
/// endpoints. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="BBCNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class BbcNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "BBCNews";
    public const string ClientName = "BbcNewsRssClient";

    public BbcNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<BbcNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
