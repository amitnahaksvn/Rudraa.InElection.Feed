using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// CBS News (cbsnews.com, United States) RSS integration - cbsnews.com/latest/rss/{section}.
/// Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="CBSNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class CbsNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "CBSNews";
    public const string ClientName = "CbsNewsRssClient";

    public CbsNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<CbsNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
