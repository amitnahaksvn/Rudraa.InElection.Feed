using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Sydsvenskan (sydsvenskan.se, Sweden - Swedish-language) RSS integration -
/// sydsvenskan.se/rss.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Sweden"]:Providers[Name="Sydsvenskan"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class SydsvenskanRssProvider : BaseRssProvider
{
    public const string ProviderName = "Sydsvenskan";
    public const string ClientName = "SydsvenskanRssClient";

    public SydsvenskanRssProvider(IHttpClientFactory httpClientFactory, ILogger<SydsvenskanRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
