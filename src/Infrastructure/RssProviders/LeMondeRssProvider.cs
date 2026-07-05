using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Le Monde (lemonde.fr, France - French-language edition) RSS integration -
/// lemonde.fr/{section}/rss_full.xml. The requested English-edition feed
/// (lemonde.fr/en/rss/full.xml) 404s - only the French-language section feeds work. Feed URLs
/// live entirely in configuration under
/// NewsCrawler:Countries[Name="France"]:Providers[Name="LeMonde"]:Feeds, never hardcoded here.
/// </summary>
public sealed class LeMondeRssProvider : BaseRssProvider
{
    public const string ProviderName = "LeMonde";
    public const string ClientName = "LeMondeRssClient";

    public LeMondeRssProvider(IHttpClientFactory httpClientFactory, ILogger<LeMondeRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
