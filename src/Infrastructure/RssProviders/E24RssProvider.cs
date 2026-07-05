using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// E24 (e24.no, Norway - Norwegian-language business daily) RSS integration - e24.no/rss.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Norway"]:Providers[Name="E24"]:Feeds, never hardcoded here.
/// </summary>
public sealed class E24RssProvider : BaseRssProvider
{
    public const string ProviderName = "E24";
    public const string ClientName = "E24RssClient";

    public E24RssProvider(IHttpClientFactory httpClientFactory, ILogger<E24RssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
