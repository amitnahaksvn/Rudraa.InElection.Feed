using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Europa Press (europapress.es, Spain) RSS integration - europapress.es/rss/rss.aspx.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Spain"]:Providers[Name="EuropaPress"]:Feeds, never hardcoded here.
/// </summary>
public sealed class EuropaPressRssProvider : BaseRssProvider
{
    public const string ProviderName = "EuropaPress";
    public const string ClientName = "EuropaPressRssClient";

    public EuropaPressRssProvider(IHttpClientFactory httpClientFactory, ILogger<EuropaPressRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
