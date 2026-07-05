using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Dagens Nyheter (dn.se, Sweden - Swedish-language) RSS integration - dn.se/nyheter/m/rss/.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Sweden"]:Providers[Name="DagensNyheter"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class DagensNyheterRssProvider : BaseRssProvider
{
    public const string ProviderName = "DagensNyheter";
    public const string ClientName = "DagensNyheterRssClient";

    public DagensNyheterRssProvider(IHttpClientFactory httpClientFactory, ILogger<DagensNyheterRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
