using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ANSA (ansa.it, Italy - English-language edition) RSS integration -
/// ansa.it/english/english_rss.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Italy"]:Providers[Name="ANSA"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AnsaRssProvider : BaseRssProvider
{
    public const string ProviderName = "ANSA";
    public const string ClientName = "AnsaRssClient";

    public AnsaRssProvider(IHttpClientFactory httpClientFactory, ILogger<AnsaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
