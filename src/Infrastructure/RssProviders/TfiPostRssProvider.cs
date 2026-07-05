using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// TFIPOST (tfipost.com) RSS integration - standard WordPress /feed/. No image tags, relies on
/// the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TFIPost"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TfiPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "TFIPost";
    public const string ClientName = "TfiPostRssClient";

    public TfiPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<TfiPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
