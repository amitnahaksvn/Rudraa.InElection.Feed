using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Manipur.org (manipur.org) RSS integration - a Manipur-focused news aggregator (its own feed
/// items are syndicated from other outlets such as The Indian Express, Times of India, and
/// Northeast Today, confirmed by title/content sampling), standard WordPress /news/feed/. The
/// identical URL also appeared under a "Meghalaya" row in the source list this provider was added
/// from, but every sampled item is Manipur-specific, not Meghalaya-specific - that second listing
/// was a copy/paste mislabel and was not wired in as a Meghalaya source. Feed URL lives entirely in
/// configuration under NewsCrawler:Providers[Name="ManipurOrg"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ManipurOrgRssProvider : BaseRssProvider
{
    public const string ProviderName = "ManipurOrg";
    public const string ClientName = "ManipurOrgRssClient";

    public ManipurOrgRssProvider(IHttpClientFactory httpClientFactory, ILogger<ManipurOrgRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
