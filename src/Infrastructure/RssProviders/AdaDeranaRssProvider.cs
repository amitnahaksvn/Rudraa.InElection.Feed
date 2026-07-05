using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Ada Derana (adaderana.lk, Sri Lanka - English-language) RSS integration -
/// adaderana.lk/rss.php. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Sri Lanka"]:Providers[Name="AdaDerana"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class AdaDeranaRssProvider : BaseRssProvider
{
    public const string ProviderName = "AdaDerana";
    public const string ClientName = "AdaDeranaRssClient";

    public AdaDeranaRssProvider(IHttpClientFactory httpClientFactory, ILogger<AdaDeranaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
