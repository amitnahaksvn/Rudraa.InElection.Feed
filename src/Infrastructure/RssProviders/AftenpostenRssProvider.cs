using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Aftenposten (aftenposten.no, Norway - Norwegian-language) RSS integration -
/// aftenposten.no/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Norway"]:Providers[Name="Aftenposten"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class AftenpostenRssProvider : BaseRssProvider
{
    public const string ProviderName = "Aftenposten";
    public const string ClientName = "AftenpostenRssClient";

    public AftenpostenRssProvider(IHttpClientFactory httpClientFactory, ILogger<AftenpostenRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
