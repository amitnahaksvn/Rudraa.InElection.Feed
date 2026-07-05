using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RT (rt.com, Russia - English-language, state-affiliated edition) RSS integration -
/// rt.com/rss/{section}. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="Russia"]:Providers[Name="RT"]:Feeds, never hardcoded here.
/// </summary>
public sealed class RtRssProvider : BaseRssProvider
{
    public const string ProviderName = "RT";
    public const string ClientName = "RtRssClient";

    public RtRssProvider(IHttpClientFactory httpClientFactory, ILogger<RtRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
