using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Yonhap News Agency (yna.co.kr, South Korea) RSS integration - the English edition at
/// en.yna.co.kr/RSS/news.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="Yonhap"]:Feeds, never hardcoded here.
/// </summary>
public sealed class YonhapRssProvider : BaseRssProvider
{
    public const string ProviderName = "Yonhap";
    public const string ClientName = "YonhapRssClient";

    public YonhapRssProvider(IHttpClientFactory httpClientFactory, ILogger<YonhapRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
