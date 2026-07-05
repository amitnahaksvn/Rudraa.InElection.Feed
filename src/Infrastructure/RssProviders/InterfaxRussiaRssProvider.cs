using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Interfax (interfax.ru, Russia - Russian-language edition) RSS integration -
/// interfax.ru/rss.asp. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Russia"]:Providers[Name="InterfaxRussia"]:Feeds, never hardcoded here.
/// </summary>
public sealed class InterfaxRussiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "InterfaxRussia";
    public const string ClientName = "InterfaxRussiaRssClient";

    public InterfaxRussiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<InterfaxRussiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
