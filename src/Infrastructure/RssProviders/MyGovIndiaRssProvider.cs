using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// MyGov India (mygov.in), the citizen engagement platform of the Government of India - the bare
/// /rss.xml endpoint. Content is a mix of campaign announcements, ministry group listings, and
/// blog posts rather than pure news, reflecting what the platform itself actually is; accepted
/// as-is rather than filtered. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="MyGovIndia"]:Feeds, never hardcoded here.
/// </summary>
public sealed class MyGovIndiaRssProvider : BaseRssProvider
{
    public const string ProviderName = "MyGovIndia";
    public const string ClientName = "MyGovIndiaRssClient";

    public MyGovIndiaRssProvider(IHttpClientFactory httpClientFactory, ILogger<MyGovIndiaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
