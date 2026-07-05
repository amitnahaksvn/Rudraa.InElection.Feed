using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// France Info (francetvinfo.fr, France - French-language edition) RSS integration -
/// francetvinfo.fr/titres.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="France"]:Providers[Name="FranceInfo"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FranceInfoRssProvider : BaseRssProvider
{
    public const string ProviderName = "FranceInfo";
    public const string ClientName = "FranceInfoRssClient";

    public FranceInfoRssProvider(IHttpClientFactory httpClientFactory, ILogger<FranceInfoRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
