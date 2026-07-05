using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ABC News (abc.net.au, Australia - Australian Broadcasting Corporation, distinct from the
/// American ABC network) RSS integration - abc.net.au/news/feed/51120/rss.xml. Feed URL lives
/// entirely in configuration under NewsCrawler:Providers[Name="ABCNewsAustralia"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class AbcNewsAustraliaRssProvider : BaseRssProvider
{
    public const string ProviderName = "ABCNewsAustralia";
    public const string ClientName = "AbcNewsAustraliaRssClient";

    public AbcNewsAustraliaRssProvider(IHttpClientFactory httpClientFactory, ILogger<AbcNewsAustraliaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
