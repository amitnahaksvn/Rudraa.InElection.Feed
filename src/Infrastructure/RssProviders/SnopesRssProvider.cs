using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Snopes (snopes.com, English-language fact-checking) RSS integration - snopes.com/feed/. Global
/// coverage, not tied to one nation, so it's wired under the same "International" pseudo-country
/// as UN News above. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="International"]:Providers[Name="Snopes"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class SnopesRssProvider : BaseRssProvider
{
    public const string ProviderName = "Snopes";
    public const string ClientName = "SnopesRssClient";

    public SnopesRssProvider(IHttpClientFactory httpClientFactory, ILogger<SnopesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
