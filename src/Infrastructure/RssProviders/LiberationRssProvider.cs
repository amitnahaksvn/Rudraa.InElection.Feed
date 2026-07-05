using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Libération (liberation.fr, France - French-language edition) RSS integration -
/// liberation.fr/arc/outboundfeeds/rss/ (Arc XP CMS pattern). Feed URL lives entirely in
/// configuration under NewsCrawler:Countries[Name="France"]:Providers[Name="Liberation"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class LiberationRssProvider : BaseRssProvider
{
    public const string ProviderName = "Liberation";
    public const string ClientName = "LiberationRssClient";

    public LiberationRssProvider(IHttpClientFactory httpClientFactory, ILogger<LiberationRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
