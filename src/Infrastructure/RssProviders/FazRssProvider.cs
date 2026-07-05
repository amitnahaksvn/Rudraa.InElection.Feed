using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// FAZ / Frankfurter Allgemeine Zeitung (faz.net, Germany - German-language edition) RSS
/// integration - faz.net/rss/aktuell/{section}/. Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="Germany"]:Providers[Name="FAZ"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FazRssProvider : BaseRssProvider
{
    public const string ProviderName = "FAZ";
    public const string ClientName = "FazRssClient";

    public FazRssProvider(IHttpClientFactory httpClientFactory, ILogger<FazRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
