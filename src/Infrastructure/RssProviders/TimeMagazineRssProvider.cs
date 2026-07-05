using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// TIME (time.com, United States) RSS integration - standard WordPress /feed. Feed URL lives
/// entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="Time"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TimeMagazineRssProvider : BaseRssProvider
{
    public const string ProviderName = "Time";
    public const string ClientName = "TimeMagazineRssClient";

    public TimeMagazineRssProvider(IHttpClientFactory httpClientFactory, ILogger<TimeMagazineRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
