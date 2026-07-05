using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// HVG (hvg.hu, Hungary - Hungarian-language) RSS integration - hvg.hu/rss.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Hungary"]:Providers[Name="HVG"]:Feeds, never hardcoded here.
/// </summary>
public sealed class HvgRssProvider : BaseRssProvider
{
    public const string ProviderName = "HVG";
    public const string ClientName = "HvgRssClient";

    public HvgRssProvider(IHttpClientFactory httpClientFactory, ILogger<HvgRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
