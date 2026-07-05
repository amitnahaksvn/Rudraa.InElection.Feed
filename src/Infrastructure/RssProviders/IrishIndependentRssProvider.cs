using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Irish Independent (independent.ie, Ireland - English-language) RSS integration -
/// independent.ie/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Ireland"]:Providers[Name="IrishIndependent"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class IrishIndependentRssProvider : BaseRssProvider
{
    public const string ProviderName = "IrishIndependent";
    public const string ClientName = "IrishIndependentRssClient";

    public IrishIndependentRssProvider(IHttpClientFactory httpClientFactory, ILogger<IrishIndependentRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
