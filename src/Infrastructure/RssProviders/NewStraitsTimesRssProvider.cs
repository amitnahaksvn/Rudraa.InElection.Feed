using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// New Straits Times (nst.com.my, Malaysia - English-language) RSS integration - the requested
/// nst.com.my/rss 404s; the real feed is the standard WordPress /feed. Feed URL lives entirely
/// in configuration under
/// NewsCrawler:Countries[Name="Malaysia"]:Providers[Name="NewStraitsTimes"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class NewStraitsTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "NewStraitsTimes";
    public const string ClientName = "NewStraitsTimesRssClient";

    public NewStraitsTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<NewStraitsTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
