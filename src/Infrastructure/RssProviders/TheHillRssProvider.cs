using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Hill (thehill.com, United States - Congress/White House politics) RSS integration -
/// standard WordPress /feed, a 100-item aggregate. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="TheHill"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TheHillRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheHill";
    public const string ClientName = "TheHillRssClient";

    public TheHillRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheHillRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
