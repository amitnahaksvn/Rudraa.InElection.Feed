using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// OpIndia (opindia.com) RSS integration - standard WordPress /feed/. No image tags, relies on
/// the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="OpIndia"]:Feeds, never hardcoded here.
/// </summary>
public sealed class OpIndiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "OpIndia";
    public const string ClientName = "OpIndiaRssClient";

    public OpIndiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<OpIndiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
