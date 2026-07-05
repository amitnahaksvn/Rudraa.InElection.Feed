using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Corriere della Sera (corriere.it, Italy) RSS integration -
/// xml2.corriereobjects.it/rss/homepage.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Italy"]:Providers[Name="CorriereDellaSera"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class CorriereDellaSeraRssProvider : BaseRssProvider
{
    public const string ProviderName = "CorriereDellaSera";
    public const string ClientName = "CorriereDellaSeraRssClient";

    public CorriereDellaSeraRssProvider(IHttpClientFactory httpClientFactory, ILogger<CorriereDellaSeraRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
