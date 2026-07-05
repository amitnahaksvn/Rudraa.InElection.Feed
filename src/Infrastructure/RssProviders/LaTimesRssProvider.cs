using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Los Angeles Times (latimes.com, United States) RSS integration -
/// latimes.com/world-nation/rss2.0.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="LATimes"]:Feeds, never hardcoded here.
/// </summary>
public sealed class LaTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "LATimes";
    public const string ClientName = "LaTimesRssClient";

    public LaTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<LaTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
