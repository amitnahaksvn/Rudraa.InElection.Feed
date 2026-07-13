using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Andaman Sheekha (andamansheekha.com) RSS integration - an Andaman and Nicobar Islands-only
/// publisher, standard WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="AndamanSheekha"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AndamanSheekhaRssProvider : BaseRssProvider
{
    public const string ProviderName = "AndamanSheekha";
    public const string ClientName = "AndamanSheekhaRssClient";

    public AndamanSheekhaRssProvider(IHttpClientFactory httpClientFactory, ILogger<AndamanSheekhaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
