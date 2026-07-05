using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// IRNA (en.irna.ir, Iran - English-language state news agency) RSS integration -
/// en.irna.ir/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Iran"]:Providers[Name="IRNA"]:Feeds, never hardcoded here.
/// </summary>
public sealed class IrnaRssProvider : BaseRssProvider
{
    public const string ProviderName = "IRNA";
    public const string ClientName = "IrnaRssClient";

    public IrnaRssProvider(IHttpClientFactory httpClientFactory, ILogger<IrnaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
