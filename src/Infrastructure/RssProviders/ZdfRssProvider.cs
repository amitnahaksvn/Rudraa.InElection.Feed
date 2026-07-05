using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ZDF (zdf.de, Germany - German-language edition, ZDF's news feed) RSS integration -
/// zdf.de/rss/zdf/nachrichten. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Germany"]:Providers[Name="ZDF"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ZdfRssProvider : BaseRssProvider
{
    public const string ProviderName = "ZDF";
    public const string ClientName = "ZdfRssClient";

    public ZdfRssProvider(IHttpClientFactory httpClientFactory, ILogger<ZdfRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
