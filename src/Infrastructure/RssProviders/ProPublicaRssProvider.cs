using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ProPublica (propublica.org, United States - investigative journalism) RSS integration -
/// propublica.org/feeds/propublica/main. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="ProPublica"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class ProPublicaRssProvider : BaseRssProvider
{
    public const string ProviderName = "ProPublica";
    public const string ClientName = "ProPublicaRssClient";

    public ProPublicaRssProvider(IHttpClientFactory httpClientFactory, ILogger<ProPublicaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
