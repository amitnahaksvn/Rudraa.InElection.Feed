using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// El Nacional (elnacional.com, Venezuela - Spanish-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Venezuela"]:Providers[Name="ElNacional"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class ElNacionalRssProvider : BaseRssProvider
{
    public const string ProviderName = "ElNacional";
    public const string ClientName = "ElNacionalRssClient";

    public ElNacionalRssProvider(IHttpClientFactory httpClientFactory, ILogger<ElNacionalRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
