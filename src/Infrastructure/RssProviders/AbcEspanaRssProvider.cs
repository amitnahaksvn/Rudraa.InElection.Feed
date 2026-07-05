using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// ABC (abc.es, Spain - unrelated to the US ABC News/Australia ABC News already in this
/// codebase, named ...Espana to disambiguate) RSS integration -
/// abc.es/rss/feeds/abcPortada.xml. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Spain"]:Providers[Name="AbcEspana"]:Feeds, never hardcoded here.
/// </summary>
public sealed class AbcEspanaRssProvider : BaseRssProvider
{
    public const string ProviderName = "AbcEspana";
    public const string ClientName = "AbcEspanaRssClient";

    public AbcEspanaRssProvider(IHttpClientFactory httpClientFactory, ILogger<AbcEspanaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
