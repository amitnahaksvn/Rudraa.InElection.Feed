using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RFI / Radio France Internationale (rfi.fr, France - English-language edition) RSS
/// integration - rfi.fr/en/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="France"]:Providers[Name="RFI"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RfiRssProvider : BaseRssProvider
{
    public const string ProviderName = "RFI";
    public const string ClientName = "RfiRssClient";

    public RfiRssProvider(IHttpClientFactory httpClientFactory, ILogger<RfiRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
