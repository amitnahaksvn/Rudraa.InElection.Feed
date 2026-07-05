using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Aftonbladet (aftonbladet.se, Sweden - Swedish-language tabloid) RSS integration -
/// rss.aftonbladet.se/rss2/small/pages/sections/senastenytt/. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Sweden"]:Providers[Name="Aftonbladet"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class AftonbladetRssProvider : BaseRssProvider
{
    public const string ProviderName = "Aftonbladet";
    public const string ClientName = "AftonbladetRssClient";

    public AftonbladetRssProvider(IHttpClientFactory httpClientFactory, ILogger<AftonbladetRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
