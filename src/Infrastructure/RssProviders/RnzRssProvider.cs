using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RNZ (rnz.co.nz, New Zealand - English-language public broadcaster) RSS integration -
/// rnz.co.nz/rss/news.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="New Zealand"]:Providers[Name="RNZ"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RnzRssProvider : BaseRssProvider
{
    public const string ProviderName = "RNZ";
    public const string ClientName = "RnzRssClient";

    public RnzRssProvider(IHttpClientFactory httpClientFactory, ILogger<RnzRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
