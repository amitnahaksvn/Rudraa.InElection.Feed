using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// DR News (dr.dk, Denmark - Danish-language public broadcaster) RSS integration -
/// dr.dk/nyheder/service/feeds/senestenyt. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Denmark"]:Providers[Name="DRNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class DrNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "DRNews";
    public const string ClientName = "DrNewsRssClient";

    public DrNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<DrNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
