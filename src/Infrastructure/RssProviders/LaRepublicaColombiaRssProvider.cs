using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// La Republica (larepublica.co, Colombia - Spanish-language business daily; named
/// ...Colombia to disambiguate from Peru's own, unrelated "La Republica" - larepublica.pe -
/// elsewhere in this codebase) RSS integration - larepublica.co/rss. Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Colombia"]:Providers[Name="LaRepublicaColombia"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class LaRepublicaColombiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "LaRepublicaColombia";
    public const string ClientName = "LaRepublicaColombiaRssClient";

    public LaRepublicaColombiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<LaRepublicaColombiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
