using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Daily News Egypt (dailynewsegypt.com, Egypt - English-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Egypt"]:Providers[Name="DailyNewsEgypt"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class DailyNewsEgyptRssProvider : BaseRssProvider
{
    public const string ProviderName = "DailyNewsEgypt";
    public const string ClientName = "DailyNewsEgyptRssClient";

    public DailyNewsEgyptRssProvider(IHttpClientFactory httpClientFactory, ILogger<DailyNewsEgyptRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
