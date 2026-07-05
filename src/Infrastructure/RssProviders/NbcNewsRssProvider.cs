using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NBC News (nbcnews.com, United States) RSS integration - the classic feeds.nbcnews.com
/// endpoints. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="NBCNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NbcNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "NBCNews";
    public const string ClientName = "NbcNewsRssClient";

    public NbcNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<NbcNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
