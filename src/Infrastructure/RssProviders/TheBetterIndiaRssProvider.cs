using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Better India (thebetterindia.com, positive/human-interest news) RSS integration - standard
/// WordPress /feed/ (redirects once, followed transparently by HttpClient's default behavior).
/// Feed URL lives entirely in configuration under NewsCrawler:Providers[Name="TheBetterIndia"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class TheBetterIndiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheBetterIndia";
    public const string ClientName = "TheBetterIndiaRssClient";

    public TheBetterIndiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheBetterIndiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
