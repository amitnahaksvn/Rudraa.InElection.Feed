using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Seznam Zpravy (seznamzpravy.cz, Czech Republic - Czech-language) RSS integration -
/// seznamzpravy.cz/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Czech Republic"]:Providers[Name="SeznamZpravy"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class SeznamZpravyRssProvider : BaseRssProvider
{
    public const string ProviderName = "SeznamZpravy";
    public const string ClientName = "SeznamZpravyRssClient";

    public SeznamZpravyRssProvider(IHttpClientFactory httpClientFactory, ILogger<SeznamZpravyRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
