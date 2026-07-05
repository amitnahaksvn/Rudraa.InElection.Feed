using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Vox (vox.com, United States) RSS integration - vox.com/rss/index.xml. ISO-8601 pubDate
/// format, still parseable by the default tier of BaseRssProvider.ParsePublishDate. Feed URL
/// lives entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="Vox"]:Feeds, never hardcoded here.
/// </summary>
public sealed class VoxRssProvider : BaseRssProvider
{
    public const string ProviderName = "Vox";
    public const string ClientName = "VoxRssClient";

    public VoxRssProvider(IHttpClientFactory httpClientFactory, ILogger<VoxRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
