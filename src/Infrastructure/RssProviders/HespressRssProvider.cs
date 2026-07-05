using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Hespress (hespress.com, Morocco - Arabic-language) RSS integration - standard WordPress
/// /feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Morocco"]:Providers[Name="Hespress"]:Feeds, never hardcoded here.
/// </summary>
public sealed class HespressRssProvider : BaseRssProvider
{
    public const string ProviderName = "Hespress";
    public const string ClientName = "HespressRssClient";

    public HespressRssProvider(IHttpClientFactory httpClientFactory, ILogger<HespressRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
