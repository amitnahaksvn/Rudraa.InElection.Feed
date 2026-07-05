using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NRK (nrk.no, Norway - Norwegian-language public broadcaster) RSS integration -
/// nrk.no/toppsaker.rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Norway"]:Providers[Name="NRK"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NrkRssProvider : BaseRssProvider
{
    public const string ProviderName = "NRK";
    public const string ClientName = "NrkRssClient";

    public NrkRssProvider(IHttpClientFactory httpClientFactory, ILogger<NrkRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
