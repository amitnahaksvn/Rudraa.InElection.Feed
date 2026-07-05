using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// President of India (presidentofindia.gov.in, English-language) RSS integration -
/// presidentofindia.gov.in/rss.xml - genuinely live, 10 recent items (presidential speeches/
/// engagements) at verification time. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="India"]:Providers[Name="PresidentOfIndia"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class PresidentOfIndiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "PresidentOfIndia";
    public const string ClientName = "PresidentOfIndiaRssClient";

    public PresidentOfIndiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<PresidentOfIndiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
