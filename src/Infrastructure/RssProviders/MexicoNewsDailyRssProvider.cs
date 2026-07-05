using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Mexico News Daily (mexiconewsdaily.com, Mexico) RSS integration - standard WordPress /feed/.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="MexicoNewsDaily"]:Feeds, never hardcoded here.
/// </summary>
public sealed class MexicoNewsDailyRssProvider : BaseRssProvider
{
    public const string ProviderName = "MexicoNewsDaily";
    public const string ClientName = "MexicoNewsDailyRssClient";

    public MexicoNewsDailyRssProvider(IHttpClientFactory httpClientFactory, ILogger<MexicoNewsDailyRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
