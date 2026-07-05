using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// VietnamPlus (en.vietnamplus.vn, Vietnam - English-language) RSS integration -
/// en.vietnamplus.vn/rss/home.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Vietnam"]:Providers[Name="VietnamPlus"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class VietnamPlusRssProvider : BaseRssProvider
{
    public const string ProviderName = "VietnamPlus";
    public const string ClientName = "VietnamPlusRssClient";

    public VietnamPlusRssProvider(IHttpClientFactory httpClientFactory, ILogger<VietnamPlusRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
