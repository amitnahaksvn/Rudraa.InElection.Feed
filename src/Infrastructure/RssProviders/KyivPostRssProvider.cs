using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Kyiv Post (kyivpost.com, Ukraine - English-language edition) RSS integration - standard
/// WordPress /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Ukraine"]:Providers[Name="KyivPost"]:Feeds, never hardcoded here.
/// </summary>
public sealed class KyivPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "KyivPost";
    public const string ClientName = "KyivPostRssClient";

    public KyivPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<KyivPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
