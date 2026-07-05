using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Eleven Media (elevenmyanmar.com, Myanmar - English-language) RSS integration -
/// elevenmyanmar.com/rss.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Myanmar"]:Providers[Name="ElevenMedia"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class ElevenMediaRssProvider : BaseRssProvider
{
    public const string ProviderName = "ElevenMedia";
    public const string ClientName = "ElevenMediaRssClient";

    public ElevenMediaRssProvider(IHttpClientFactory httpClientFactory, ILogger<ElevenMediaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
