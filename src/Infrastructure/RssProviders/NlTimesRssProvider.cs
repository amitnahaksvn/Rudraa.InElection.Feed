using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NL Times (nltimes.nl, Netherlands - English-language edition) RSS integration -
/// nltimes.nl/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Netherlands"]:Providers[Name="NLTimes"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class NlTimesRssProvider : BaseRssProvider
{
    public const string ProviderName = "NLTimes";
    public const string ClientName = "NlTimesRssClient";

    public NlTimesRssProvider(IHttpClientFactory httpClientFactory, ILogger<NlTimesRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
