using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// VnExpress (vnexpress.net, Vietnam - Vietnamese-language) RSS integration -
/// vnexpress.net/rss/tin-moi-nhat.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Vietnam"]:Providers[Name="VnExpress"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class VnExpressRssProvider : BaseRssProvider
{
    public const string ProviderName = "VnExpress";
    public const string ClientName = "VnExpressRssClient";

    public VnExpressRssProvider(IHttpClientFactory httpClientFactory, ILogger<VnExpressRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
