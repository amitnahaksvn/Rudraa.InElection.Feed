using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Buenos Aires Times (batimes.com.ar, Argentina - English-language) RSS integration - standard
/// WordPress /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Argentina"]:Providers[Name="BuenosAiresTimes"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class BuenosAiresTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "BuenosAiresTimes";
    public const string ClientName = "BuenosAiresTimesRssClient";

    public BuenosAiresTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<BuenosAiresTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
