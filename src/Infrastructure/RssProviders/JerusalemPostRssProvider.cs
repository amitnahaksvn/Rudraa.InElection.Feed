using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Jerusalem Post (jpost.com, Israel) RSS integration -
/// jpost.com/rss/rssfeedsfrontpage.aspx. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="JerusalemPost"]:Feeds, never hardcoded here.
/// </summary>
public sealed class JerusalemPostRssProvider : BaseRssProvider
{
    public const string ProviderName = "JerusalemPost";
    public const string ClientName = "JerusalemPostRssClient";

    public JerusalemPostRssProvider(IHttpClientFactory httpClientFactory, ILogger<JerusalemPostRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
