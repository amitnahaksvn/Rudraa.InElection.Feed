using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Business Insider (businessinsider.com, United States) RSS integration -
/// businessinsider.com/rss. ISO-8601 pubDate format, still parseable by the default tier of
/// BaseRssProvider.ParsePublishDate. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="BusinessInsider"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class BusinessInsiderRssProvider : BaseRssProvider
{
    public const string ProviderName = "BusinessInsider";
    public const string ClientName = "BusinessInsiderRssClient";

    public BusinessInsiderRssProvider(IHttpClientFactory httpClientFactory, ILogger<BusinessInsiderRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
