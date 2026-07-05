using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Punch (punchng.com, Nigeria - English-language) RSS integration - standard WordPress
/// /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Nigeria"]:Providers[Name="Punch"]:Feeds, never hardcoded here.
/// </summary>
public sealed class PunchRssProvider : BaseRssProvider
{
    public const string ProviderName = "Punch";
    public const string ClientName = "PunchRssClient";

    public PunchRssProvider(IHttpClientFactory httpClientFactory, ILogger<PunchRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
