using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NOS (nos.nl, Netherlands - Dutch-language public broadcaster) RSS integration -
/// feeds.nos.nl/nosnieuwsalgemeen. The requested second "Politics" feed
/// (feeds.nos.nl/nospolitiek) 404s and no discoverable alternative exists on nos.nl's own
/// homepage, so this provider ships with only the general-news feed. Feed URL lives entirely
/// in configuration under
/// NewsCrawler:Countries[Name="Netherlands"]:Providers[Name="NOS"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NosRssProvider : BaseRssProvider
{
    public const string ProviderName = "NOS";
    public const string ClientName = "NosRssClient";

    public NosRssProvider(IHttpClientFactory httpClientFactory, ILogger<NosRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
