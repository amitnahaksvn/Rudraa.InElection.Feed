using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Fox News (foxnews.com, United States) RSS integration - the classic feeds.foxnews.com
/// endpoints. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="FoxNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FoxNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "FoxNews";
    public const string ClientName = "FoxNewsRssClient";

    public FoxNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<FoxNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
