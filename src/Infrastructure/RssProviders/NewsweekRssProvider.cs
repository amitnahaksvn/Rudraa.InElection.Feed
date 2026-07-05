using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Newsweek (newsweek.com, United States) RSS integration - newsweek.com/rss. Feed URL lives
/// entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="Newsweek"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NewsweekRssProvider : BaseRssProvider
{
    public const string ProviderName = "Newsweek";
    public const string ClientName = "NewsweekRssClient";

    public NewsweekRssProvider(IHttpClientFactory httpClientFactory, ILogger<NewsweekRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
