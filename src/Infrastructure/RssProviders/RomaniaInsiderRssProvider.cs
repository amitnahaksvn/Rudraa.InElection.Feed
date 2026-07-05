using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Romania Insider (romania-insider.com, Romania - English-language) RSS integration - the
/// requested /rss 404s; the real feed is the standard WordPress /feed. Feed URL lives entirely
/// in configuration under
/// NewsCrawler:Countries[Name="Romania"]:Providers[Name="RomaniaInsider"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class RomaniaInsiderRssProvider : BaseRssProvider
{
    public const string ProviderName = "RomaniaInsider";
    public const string ClientName = "RomaniaInsiderRssClient";

    public RomaniaInsiderRssProvider(IHttpClientFactory httpClientFactory, ILogger<RomaniaInsiderRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
