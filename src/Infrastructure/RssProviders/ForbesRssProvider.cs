using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Forbes (forbes.com, United States) RSS integration - only the /business/feed/ category is
/// wired up; /most-popular/feed/ returns real RSS but every item is stale (dated Jan-Oct 2024, an
/// evergreen "most read" list rather than recent news), same "technically 200 but dead" trap as
/// CNN/Xinhua/China Daily, so it's deliberately excluded. Feed URL lives entirely in
/// configuration under NewsCrawler:Countries[Name="United States"]:Providers[Name="Forbes"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class ForbesRssProvider : BaseRssProvider
{
    public const string ProviderName = "Forbes";
    public const string ClientName = "ForbesRssClient";

    public ForbesRssProvider(IHttpClientFactory httpClientFactory, ILogger<ForbesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
