using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The News International (thenews.com.pk, Pakistan - English-language) RSS integration -
/// thenews.com.pk/rss/1/10. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Pakistan"]:Providers[Name="TheNewsInternational"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class TheNewsInternationalRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheNewsInternational";
    public const string ClientName = "TheNewsInternationalRssClient";

    public TheNewsInternationalRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheNewsInternationalRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
