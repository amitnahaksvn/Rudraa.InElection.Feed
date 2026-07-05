using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Reforma (reforma.com, Mexico - Spanish-language edition) RSS integration -
/// reforma.com/rss/portada.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Mexico"]:Providers[Name="Reforma"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ReformaRssProvider : BaseRssProvider
{
    public const string ProviderName = "Reforma";
    public const string ClientName = "ReformaRssClient";

    public ReformaRssProvider(IHttpClientFactory httpClientFactory, ILogger<ReformaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
