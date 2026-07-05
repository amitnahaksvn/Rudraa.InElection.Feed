using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// India.com RSS integration - standard WordPress /feed/. No image tags, relies on the og:image
/// HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="IndiaDotCom"]:Feeds, never hardcoded here.
/// </summary>
public sealed class IndiaDotComRssProvider : BaseRssProvider
{
    public const string ProviderName = "IndiaDotCom";
    public const string ClientName = "IndiaDotComRssClient";

    public IndiaDotComRssProvider(IHttpClientFactory httpClientFactory, ILogger<IndiaDotComRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
