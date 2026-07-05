using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Rappler (rappler.com, Philippines - English-language) RSS integration - rappler.com/rss/.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Philippines"]:Providers[Name="Rappler"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class RapplerRssProvider : BaseRssProvider
{
    public const string ProviderName = "Rappler";
    public const string ClientName = "RapplerRssClient";

    public RapplerRssProvider(IHttpClientFactory httpClientFactory, ILogger<RapplerRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
