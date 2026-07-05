using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Fortune (fortune.com, United States) RSS integration - standard WordPress /feed. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="Fortune"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FortuneRssProvider : BaseRssProvider
{
    public const string ProviderName = "Fortune";
    public const string ClientName = "FortuneRssClient";

    public FortuneRssProvider(IHttpClientFactory httpClientFactory, ILogger<FortuneRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
