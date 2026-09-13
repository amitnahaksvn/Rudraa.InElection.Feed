using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// News18 (news18.com) RSS integration. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="News18"]:Feeds, never hardcoded here. News18's CDN (Akamai)
/// returns 403 for crawler-style User-Agents while serving the same public feeds to browser
/// UAs, so this provider's named HttpClient is registered with a browser-style UA (see
/// InfrastructureServiceCollectionExtensions) - the only provider that differs there.
///
/// That UA fix alone stopped being enough: routed through <see cref="WaybackMachineFeedResolver"/>
/// after 668 unresolved HTTP 403s accumulated in production - confirmed the exact same "Politics"/
/// "India" feed requests (already using BrowserUserAgent) fail identically from this app's Azure
/// outbound IP by far the largest single cause of failures in the ErrorLogs collection at the time
/// this was added. Same root-cause category as <see cref="IndianExpressRssProvider"/>/
/// <see cref="MPInfoRssProvider"/>/<see cref="NdmaRssProvider"/> (an edge/network block against
/// this app's specific outbound IP, not a UA or content issue) - see
/// <see cref="WaybackMachineFeedResolver"/>'s own doc comment for the full mechanism.
/// </summary>
public sealed class News18RssProvider : BaseRssProvider
{
    public const string ProviderName = "News18";
    public const string ClientName = "News18RssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<News18RssProvider> _logger;

    public News18RssProvider(IHttpClientFactory httpClientFactory, ILogger<News18RssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
        : base(httpClientFactory, logger)
    {
        _httpClientFactory = httpClientFactory;
        _waybackOptions = waybackOptions.Value;
        _logger = logger;
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;

    protected override Task<string> ResolveFeedUrlAsync(RssFeedOptions feed, CancellationToken cancellationToken) =>
        WaybackMachineFeedResolver.ResolveAsync(
            _httpClientFactory.CreateClient(InfrastructureServiceCollectionExtensions.WaybackMachineClientName),
            _waybackOptions,
            feed.Url,
            _logger,
            cancellationToken);
}
