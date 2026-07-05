using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Kurier (kurier.at, Austria - German-language) RSS integration - the requested kurier.at/rss
/// 404s; the real feed is kurier.at/xml/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Austria"]:Providers[Name="Kurier"]:Feeds, never hardcoded here.
/// </summary>
public sealed class KurierRssProvider : BaseRssProvider
{
    public const string ProviderName = "Kurier";
    public const string ClientName = "KurierRssClient";

    public KurierRssProvider(IHttpClientFactory httpClientFactory, ILogger<KurierRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
