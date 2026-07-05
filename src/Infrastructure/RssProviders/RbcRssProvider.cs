using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RBC (rbc.ru, Russia - Russian-language business edition) RSS integration -
/// rssexport.rbc.ru/rbcnews/news/30/full.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Russia"]:Providers[Name="RBC"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RbcRssProvider : BaseRssProvider
{
    public const string ProviderName = "RBC";
    public const string ClientName = "RbcRssClient";

    public RbcRssProvider(IHttpClientFactory httpClientFactory, ILogger<RbcRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
