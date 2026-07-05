using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Der Standard (derstandard.at, Austria - German-language) RSS integration -
/// derstandard.at/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Austria"]:Providers[Name="DerStandard"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class DerStandardRssProvider : BaseRssProvider
{
    public const string ProviderName = "DerStandard";
    public const string ClientName = "DerStandardRssClient";

    public DerStandardRssProvider(IHttpClientFactory httpClientFactory, ILogger<DerStandardRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
