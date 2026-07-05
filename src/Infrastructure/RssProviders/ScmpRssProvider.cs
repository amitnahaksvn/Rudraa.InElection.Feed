using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// South China Morning Post (scmp.com, Hong Kong - English-language) RSS integration -
/// scmp.com/rss/91/feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Hong Kong"]:Providers[Name="SCMP"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ScmpRssProvider : BaseRssProvider
{
    public const string ProviderName = "SCMP";
    public const string ClientName = "ScmpRssClient";

    public ScmpRssProvider(IHttpClientFactory httpClientFactory, ILogger<ScmpRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
