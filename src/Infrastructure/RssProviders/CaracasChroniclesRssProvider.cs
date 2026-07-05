using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Caracas Chronicles (caracaschronicles.com, Venezuela - English-language) RSS integration -
/// standard WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Venezuela"]:Providers[Name="CaracasChronicles"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class CaracasChroniclesRssProvider : BaseRssProvider
{
    public const string ProviderName = "CaracasChronicles";
    public const string ClientName = "CaracasChroniclesRssClient";

    public CaracasChroniclesRssProvider(IHttpClientFactory httpClientFactory, ILogger<CaracasChroniclesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
