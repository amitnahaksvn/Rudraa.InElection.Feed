using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// El Mundo (elmundo.es, Spain) RSS integration - e00-elmundo.uecdn.es/elmundo/rss/portada.xml.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Spain"]:Providers[Name="ElMundo"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ElMundoRssProvider : BaseRssProvider
{
    public const string ProviderName = "ElMundo";
    public const string ClientName = "ElMundoRssClient";

    public ElMundoRssProvider(IHttpClientFactory httpClientFactory, ILogger<ElMundoRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
