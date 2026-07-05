using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Hindu Business Line (thehindubusinessline.com, The Hindu Group's business daily) RSS
/// integration - a plain ?service=rss query parameter on the site root. Feed URLs live entirely
/// in configuration under NewsCrawler:Providers[Name="HinduBusinessLine"]:Feeds, never hardcoded here.
/// </summary>
public sealed class HinduBusinessLineRssProvider : BaseRssProvider
{
    public const string ProviderName = "HinduBusinessLine";
    public const string ClientName = "HinduBusinessLineRssClient";

    public HinduBusinessLineRssProvider(IHttpClientFactory httpClientFactory, ILogger<HinduBusinessLineRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
