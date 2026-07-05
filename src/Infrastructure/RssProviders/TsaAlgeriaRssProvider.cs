using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// TSA Algeria (tsa-algerie.com, Algeria - French-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Algeria"]:Providers[Name="TSAAlgeria"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TsaAlgeriaRssProvider : BaseRssProvider
{
    public const string ProviderName = "TSAAlgeria";
    public const string ClientName = "TsaAlgeriaRssClient";

    public TsaAlgeriaRssProvider(IHttpClientFactory httpClientFactory, ILogger<TsaAlgeriaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
