using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NHK World (nhk.or.jp, Japan) RSS integration - www3.nhk.or.jp/rss/news/cat0.xml. No image
/// tags, relies on the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="NHKWorld"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NhkWorldRssProvider : BaseRssProvider
{
    public const string ProviderName = "NHKWorld";
    public const string ClientName = "NhkWorldRssClient";

    public NhkWorldRssProvider(IHttpClientFactory httpClientFactory, ILogger<NhkWorldRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
