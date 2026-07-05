using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// SVT News (svt.se, Sweden - Swedish-language public broadcaster) RSS integration -
/// svt.se/nyheter/rss.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Sweden"]:Providers[Name="SVTNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class SvtNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "SVTNews";
    public const string ClientName = "SvtNewsRssClient";

    public SvtNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<SvtNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
