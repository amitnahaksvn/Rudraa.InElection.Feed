using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// SBS News (sbs.com.au, Australia) RSS integration - sbs.com.au/news/feed. Feed URL lives
/// entirely in configuration under NewsCrawler:Providers[Name="SBSNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class SbsNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "SBSNews";
    public const string ClientName = "SbsNewsRssClient";

    public SbsNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<SbsNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
