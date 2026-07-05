using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// SABC News (sabcnews.com, South Africa - South African Broadcasting Corporation) RSS
/// integration - sabcnews.com/sabcnews/feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="South Africa"]:Providers[Name="SABCNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class SabcNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "SABCNews";
    public const string ClientName = "SabcNewsRssClient";

    public SabcNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<SabcNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
