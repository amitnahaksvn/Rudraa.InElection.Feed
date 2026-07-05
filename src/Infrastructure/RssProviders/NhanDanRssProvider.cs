using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Nhan Dan (en.nhandan.vn, Vietnam - English-language edition of the Communist Party's official
/// newspaper) RSS integration - en.nhandan.vn/rss/home.rss. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Vietnam"]:Providers[Name="NhanDan"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NhanDanRssProvider : BaseRssProvider
{
    public const string ProviderName = "NhanDan";
    public const string ClientName = "NhanDanRssClient";

    public NhanDanRssProvider(IHttpClientFactory httpClientFactory, ILogger<NhanDanRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
