using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// National Post (nationalpost.com, Canada) RSS integration - standard WordPress /feed. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="Canada"]:Providers[Name="NationalPost"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NationalPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "NationalPost";
    public const string ClientName = "NationalPostRssClient";

    public NationalPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<NationalPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
