using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Andina (andina.pe, Peru - English-language edition of the state news agency) RSS integration -
/// andina.pe/Ingles/rss.aspx (plain RSS 2.0 despite an application/atom+xml Content-Type
/// mislabel - harmless since <see cref="BaseRssProvider"/> never inspects Content-Type before
/// parsing). Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Peru"]:Providers[Name="Andina"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AndinaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Andina";
    public const string ClientName = "AndinaRssClient";

    public AndinaRssProvider(IHttpClientFactory httpClientFactory, ILogger<AndinaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
