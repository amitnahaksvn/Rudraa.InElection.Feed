using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Premium Times (premiumtimesng.com, Nigeria - English-language) RSS integration - standard
/// WordPress /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Nigeria"]:Providers[Name="PremiumTimes"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class PremiumTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "PremiumTimes";
    public const string ClientName = "PremiumTimesRssClient";

    public PremiumTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<PremiumTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
