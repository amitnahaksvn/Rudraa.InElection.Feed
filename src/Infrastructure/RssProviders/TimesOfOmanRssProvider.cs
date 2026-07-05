using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Times of Oman (timesofoman.com, Oman - English-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Oman"]:Providers[Name="TimesOfOman"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TimesOfOmanRssProvider : BaseRssProvider
{
    public const string ProviderName = "TimesOfOman";
    public const string ClientName = "TimesOfOmanRssClient";

    public TimesOfOmanRssProvider(IHttpClientFactory httpClientFactory, ILogger<TimesOfOmanRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
