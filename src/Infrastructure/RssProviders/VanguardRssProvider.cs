using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Vanguard (vanguardngr.com, Nigeria - English-language) RSS integration - standard WordPress
/// /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Nigeria"]:Providers[Name="Vanguard"]:Feeds, never hardcoded here.
/// </summary>
public sealed class VanguardRssProvider : BaseRssProvider
{
    public const string ProviderName = "Vanguard";
    public const string ClientName = "VanguardRssClient";

    public VanguardRssProvider(IHttpClientFactory httpClientFactory, ILogger<VanguardRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
