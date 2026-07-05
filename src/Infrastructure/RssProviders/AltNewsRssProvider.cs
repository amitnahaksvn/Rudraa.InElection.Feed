using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Alt News (altnews.in, a fact-checking outlet) RSS integration - standard WordPress /feed/.
/// No image tags, relies on the og:image HTML fallback. Feed URL lives entirely in configuration
/// under NewsCrawler:Providers[Name="AltNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AltNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "AltNews";
    public const string ClientName = "AltNewsRssClient";

    public AltNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<AltNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
