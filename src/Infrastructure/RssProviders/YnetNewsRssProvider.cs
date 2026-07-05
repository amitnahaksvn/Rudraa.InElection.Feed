using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Ynet News (ynetnews.com, Israel - English-language edition) RSS integration -
/// ynetnews.com/Integration/StoryRss2.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Israel"]:Providers[Name="YnetNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class YnetNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "YnetNews";
    public const string ClientName = "YnetNewsRssClient";

    public YnetNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<YnetNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
