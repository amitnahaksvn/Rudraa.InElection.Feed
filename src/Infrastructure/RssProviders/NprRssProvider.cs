using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NPR (npr.org, United States) RSS integration - feeds.npr.org/1001/rss.xml. Feed URL lives
/// entirely in configuration under NewsCrawler:Providers[Name="NPR"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NprRssProvider : BaseRssProvider
{
    public const string ProviderName = "NPR";
    public const string ClientName = "NprRssClient";

    public NprRssProvider(IHttpClientFactory httpClientFactory, ILogger<NprRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
