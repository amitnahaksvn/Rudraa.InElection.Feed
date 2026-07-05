using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NU.nl (nu.nl, Netherlands - Dutch-language) RSS integration - nu.nl/rss/Algemeen.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Netherlands"]:Providers[Name="NuNl"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NuNlRssProvider : BaseRssProvider
{
    public const string ProviderName = "NuNl";
    public const string ClientName = "NuNlRssClient";

    public NuNlRssProvider(IHttpClientFactory httpClientFactory, ILogger<NuNlRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
