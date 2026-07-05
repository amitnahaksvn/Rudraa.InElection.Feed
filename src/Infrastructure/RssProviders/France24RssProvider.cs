using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// France 24 (france24.com, France) RSS integration - the English edition at
/// france24.com/en/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="France24"]:Feeds, never hardcoded here.
/// </summary>
public sealed class France24RssProvider : BaseRssProvider
{
    public const string ProviderName = "France24";
    public const string ClientName = "France24RssClient";

    public France24RssProvider(IHttpClientFactory httpClientFactory, ILogger<France24RssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
