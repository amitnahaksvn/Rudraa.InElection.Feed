using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// El Pais (elpais.com, Spain) RSS integration -
/// feeds.elpais.com/mrss-s/pages/ep/site/elpais.com/portada. Only the homepage feed ("portada")
/// resolves; every other guessed section slug (internacional, espana, economia, ...) under the
/// same mrss-s path pattern 404s, so this provider ships with a single feed. Feed URL lives
/// entirely in configuration under
/// NewsCrawler:Countries[Name="Spain"]:Providers[Name="ElPais"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ElPaisRssProvider : BaseRssProvider
{
    public const string ProviderName = "ElPais";
    public const string ClientName = "ElPaisRssClient";

    public ElPaisRssProvider(IHttpClientFactory httpClientFactory, ILogger<ElPaisRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
