using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Daily Sabah (dailysabah.com, Turkey - English-language edition) RSS integration - the bare
/// `/rss` path is only an HTML index page listing category feed links (`/rss/{category}`), not a
/// feed itself, discovered the same way as GulfTimes/IndianExpress/TimesOfIndia elsewhere in this
/// file; Business/Politics/World are the real, working category feeds. Feed URLs live entirely in
/// configuration under NewsCrawler:Countries[Name="Turkey"]:Providers[Name="DailySabah"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class DailySabahRssProvider : BaseRssProvider
{
    public const string ProviderName = "DailySabah";
    public const string ClientName = "DailySabahRssClient";

    public DailySabahRssProvider(IHttpClientFactory httpClientFactory, ILogger<DailySabahRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
