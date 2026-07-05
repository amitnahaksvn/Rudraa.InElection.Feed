using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Le Figaro (lefigaro.fr, France - French-language edition) RSS integration -
/// lefigaro.fr/rss/figaro_{section}.xml. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="France"]:Providers[Name="LeFigaro"]:Feeds, never hardcoded here.
/// </summary>
public sealed class LeFigaroRssProvider : BaseRssProvider
{
    public const string ProviderName = "LeFigaro";
    public const string ClientName = "LeFigaroRssClient";

    public LeFigaroRssProvider(IHttpClientFactory httpClientFactory, ILogger<LeFigaroRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
