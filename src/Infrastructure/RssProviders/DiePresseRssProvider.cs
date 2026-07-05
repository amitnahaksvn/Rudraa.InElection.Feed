using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Die Presse (diepresse.com, Austria - German-language) RSS integration -
/// diepresse.com/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Austria"]:Providers[Name="DiePresse"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class DiePresseRssProvider : BaseRssProvider
{
    public const string ProviderName = "DiePresse";
    public const string ClientName = "DiePresseRssClient";

    public DiePresseRssProvider(IHttpClientFactory httpClientFactory, ILogger<DiePresseRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
