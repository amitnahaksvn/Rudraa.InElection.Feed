using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Age (theage.com.au, Australia) RSS integration - theage.com.au/rss/feed.xml. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Australia"]:Providers[Name="TheAge"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TheAgeRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheAge";
    public const string ClientName = "TheAgeRssClient";

    public TheAgeRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheAgeRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
