using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Times of Israel (timesofisrael.com, Israel) RSS integration - standard WordPress /feed/.
/// Feed URL lives entirely in configuration under NewsCrawler:Providers[Name="TimesOfIsrael"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class TimesOfIsraelRssProvider : BaseRssProvider
{
    public const string ProviderName = "TimesOfIsrael";
    public const string ClientName = "TimesOfIsraelRssClient";

    public TimesOfIsraelRssProvider(IHttpClientFactory httpClientFactory, ILogger<TimesOfIsraelRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
