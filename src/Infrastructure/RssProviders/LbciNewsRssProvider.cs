using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// LBCI News (lbcgroup.tv, Lebanon - English-language) RSS integration -
/// lbcgroup.tv/rss/news/en. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Lebanon"]:Providers[Name="LBCINews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class LbciNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "LBCINews";
    public const string ClientName = "LbciNewsRssClient";

    public LbciNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<LbciNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
