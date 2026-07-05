using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Hong Kong Free Press (hongkongfp.com, Hong Kong - English-language) RSS integration -
/// standard WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Hong Kong"]:Providers[Name="HongKongFreePress"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class HongKongFreePressRssProvider : BaseRssProvider
{
    public const string ProviderName = "HongKongFreePress";
    public const string ClientName = "HongKongFreePressRssClient";

    public HongKongFreePressRssProvider(IHttpClientFactory httpClientFactory, ILogger<HongKongFreePressRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
