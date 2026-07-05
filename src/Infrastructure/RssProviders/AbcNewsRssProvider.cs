using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ABC News (abcnews.go.com, United States) RSS integration - the classic feeds.abcnews.com
/// endpoints. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="ABCNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AbcNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "ABCNews";
    public const string ClientName = "AbcNewsRssClient";

    public AbcNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<AbcNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
