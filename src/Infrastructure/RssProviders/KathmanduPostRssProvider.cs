using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Kathmandu Post (kathmandupost.com, Nepal - English-language) RSS integration -
/// kathmandupost.com/rss (serves a real, well-formed RSS 2.0 body despite declaring a
/// text/html Content-Type - a labeling quirk, not a broken feed). Feed URL lives entirely in
/// configuration under
/// NewsCrawler:Countries[Name="Nepal"]:Providers[Name="KathmanduPost"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class KathmanduPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "KathmanduPost";
    public const string ClientName = "KathmanduPostRssClient";

    public KathmanduPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<KathmanduPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
