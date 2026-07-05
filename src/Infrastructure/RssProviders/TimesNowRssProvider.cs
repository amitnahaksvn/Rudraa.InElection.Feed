using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Times Now (timesnownews.com) RSS integration - a single 100-item aggregate feed
/// (timesnownews.com/feeds/gns-en-latest.xml), no section-specific variant found. No image tags,
/// relies on the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TimesNow"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TimesNowRssProvider : BaseRssProvider
{
    public const string ProviderName = "TimesNow";
    public const string ClientName = "TimesNowRssClient";

    public TimesNowRssProvider(IHttpClientFactory httpClientFactory, ILogger<TimesNowRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
