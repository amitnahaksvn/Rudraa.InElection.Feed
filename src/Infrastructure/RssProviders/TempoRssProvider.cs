using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Tempo (tempo.co, Indonesia - Indonesian-language) RSS integration - rss.tempo.co/nasional.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Indonesia"]:Providers[Name="Tempo"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TempoRssProvider : BaseRssProvider
{
    public const string ProviderName = "Tempo";
    public const string ClientName = "TempoRssClient";

    public TempoRssProvider(IHttpClientFactory httpClientFactory, ILogger<TempoRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
