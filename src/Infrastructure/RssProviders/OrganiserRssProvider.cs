using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Organiser Weekly (organiser.org) RSS integration - standard WordPress /feed/. No image tags,
/// relies on the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Providers[Name="Organiser"]:Feeds, never hardcoded here.
/// </summary>
public sealed class OrganiserRssProvider : BaseRssProvider
{
    public const string ProviderName = "Organiser";
    public const string ClientName = "OrganiserRssClient";

    public OrganiserRssProvider(IHttpClientFactory httpClientFactory, ILogger<OrganiserRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
