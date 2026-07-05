using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Mehr News (en.mehrnews.com, Iran - English-language) RSS integration - en.mehrnews.com/rss.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Iran"]:Providers[Name="MehrNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class MehrNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "MehrNews";
    public const string ClientName = "MehrNewsRssClient";

    public MehrNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<MehrNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
