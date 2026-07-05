using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// De Telegraaf (telegraaf.nl, Netherlands - Dutch-language) RSS integration -
/// telegraaf.nl/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Netherlands"]:Providers[Name="DeTelegraaf"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class DeTelegraafRssProvider : BaseRssProvider
{
    public const string ProviderName = "DeTelegraaf";
    public const string ClientName = "DeTelegraafRssClient";

    public DeTelegraafRssProvider(IHttpClientFactory httpClientFactory, ILogger<DeTelegraafRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
