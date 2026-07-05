using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Tagesschau (tagesschau.de, Germany - German-language edition, ARD's flagship news program)
/// RSS integration - tagesschau.de/xml/rss2. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Germany"]:Providers[Name="Tagesschau"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TagesschauRssProvider : BaseRssProvider
{
    public const string ProviderName = "Tagesschau";
    public const string ClientName = "TagesschauRssClient";

    public TagesschauRssProvider(IHttpClientFactory httpClientFactory, ILogger<TagesschauRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
