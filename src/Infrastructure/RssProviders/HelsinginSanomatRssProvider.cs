using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Helsingin Sanomat (hs.fi, Finland - Finnish-language) RSS integration -
/// hs.fi/rss/tuoreimmat.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Finland"]:Providers[Name="HelsinginSanomat"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class HelsinginSanomatRssProvider : BaseRssProvider
{
    public const string ProviderName = "HelsinginSanomat";
    public const string ClientName = "HelsinginSanomatRssClient";

    public HelsinginSanomatRssProvider(IHttpClientFactory httpClientFactory, ILogger<HelsinginSanomatRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
