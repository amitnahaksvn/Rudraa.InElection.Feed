using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// TechCrunch (techcrunch.com) RSS integration - standard WordPress /feed/ endpoint, no
/// section-specific feeds needed. Items carry no image tags, so images come from the og:image
/// HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="TechCrunch"]:Feeds, never hardcoded here.
/// </summary>
public sealed class TechCrunchRssProvider : BaseRssProvider
{
    public const string ProviderName = "TechCrunch";
    public const string ClientName = "TechCrunchRssClient";

    public TechCrunchRssProvider(IHttpClientFactory httpClientFactory, ILogger<TechCrunchRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
