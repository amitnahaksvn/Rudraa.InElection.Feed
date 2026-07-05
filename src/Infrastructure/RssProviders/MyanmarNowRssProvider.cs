using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Myanmar Now (myanmar-now.org, Myanmar - English-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Myanmar"]:Providers[Name="MyanmarNow"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class MyanmarNowRssProvider : BaseRssProvider
{
    public const string ProviderName = "MyanmarNow";
    public const string ClientName = "MyanmarNowRssClient";

    public MyanmarNowRssProvider(IHttpClientFactory httpClientFactory, ILogger<MyanmarNowRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
