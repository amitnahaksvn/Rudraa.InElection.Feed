using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Onet (onet.pl, Poland - Polish-language) RSS integration - the requested onet.pl/rss 404s and
/// no rel="alternate" tag exists on that homepage; the real feed lives on Onet's news subdomain
/// at wiadomosci.onet.pl/.feed (Onet's own dot-feed naming convention). Feed URL lives entirely
/// in configuration under
/// NewsCrawler:Countries[Name="Poland"]:Providers[Name="Onet"]:Feeds, never hardcoded here.
/// </summary>
public sealed class OnetRssProvider : BaseRssProvider
{
    public const string ProviderName = "Onet";
    public const string ClientName = "OnetRssClient";

    public OnetRssProvider(IHttpClientFactory httpClientFactory, ILogger<OnetRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
