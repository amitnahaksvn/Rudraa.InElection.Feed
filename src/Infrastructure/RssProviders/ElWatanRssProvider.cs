using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// El Watan (elwatan-dz.com, Algeria - French-language) RSS integration - standard WordPress
/// /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Algeria"]:Providers[Name="ElWatan"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ElWatanRssProvider : BaseRssProvider
{
    public const string ProviderName = "ElWatan";
    public const string ClientName = "ElWatanRssClient";

    public ElWatanRssProvider(IHttpClientFactory httpClientFactory, ILogger<ElWatanRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
