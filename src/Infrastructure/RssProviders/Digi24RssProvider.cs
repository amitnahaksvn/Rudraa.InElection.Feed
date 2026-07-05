using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Digi24 (digi24.ro, Romania - Romanian-language) RSS integration - digi24.ro/rss.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Romania"]:Providers[Name="Digi24"]:Feeds, never hardcoded here.
/// </summary>
public sealed class Digi24RssProvider : BaseRssProvider
{
    public const string ProviderName = "Digi24";
    public const string ClientName = "Digi24RssClient";

    public Digi24RssProvider(IHttpClientFactory httpClientFactory, ILogger<Digi24RssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
