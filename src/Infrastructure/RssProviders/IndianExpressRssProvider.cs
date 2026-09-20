using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Indian Express (indianexpress.com) RSS integration - standard WordPress feeds
/// (/section/{name}/feed/). Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="IndianExpress"]:Feeds, never hardcoded here.
///
/// indianexpress.com's CloudFront distribution occasionally returns an instant HTTP 403
/// "Request blocked" page to this app's Azure outbound IP - an IP-reputation/rate block at
/// CloudFront's edge, not a UA or content issue (confirmed live the same request succeeds from a
/// non-Azure network). Originally routed unconditionally through
/// <see cref="WaybackMachineFeedResolver"/> (see git history), which turned out to be actively
/// harmful: this app's own real daily volume for this provider collapsed from 896 articles/day to
/// 13 once every fetch was forced through Wayback's shared ~5-captures-per-URL-per-day quota, even
/// though direct access still worked most of the time - the block is intermittent, not the
/// ~100%-of-the-time block MPInfo/NDMA/PIB have. Now uses
/// <see cref="ResolveFallbackUrlAsync"/> instead - direct access is always tried first (getting
/// full volume on every fetch that would have succeeded anyway), and only a fetch that actually
/// fails retries through Wayback.
/// </summary>
public sealed class IndianExpressRssProvider : BaseRssProvider
{
    public const string ProviderName = "IndianExpress";
    public const string ClientName = "IndianExpressRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly FeedProxyOptions _proxyOptions;
    private readonly ILogger<IndianExpressRssProvider> _logger;

    public IndianExpressRssProvider(IHttpClientFactory httpClientFactory, ILogger<IndianExpressRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions, IOptions<FeedProxyOptions> proxyOptions)
        : base(httpClientFactory, logger)
    {
        _httpClientFactory = httpClientFactory;
        _waybackOptions = waybackOptions.Value;
        _proxyOptions = proxyOptions.Value;
        _logger = logger;
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;

    /// <summary>
    /// Fallback order after a failed direct fetch: the optional relay (<see cref="FeedProxyOptions"/> -
    /// an unblocked network's IP, no shared quota), then the Wayback Machine as a last resort.
    /// </summary>
    protected override async Task<IReadOnlyList<string>> ResolveFallbackUrlsAsync(RssFeedOptions feed, CancellationToken cancellationToken)
    {
        var urls = new List<string>(2);

        if (FeedProxyUrl.Build(_proxyOptions, feed.Url) is { } relayUrl)
        {
            urls.Add(relayUrl);
        }

        var wayback = await WaybackMachineFeedResolver.ResolveAsync(
            _httpClientFactory.CreateClient(InfrastructureServiceCollectionExtensions.WaybackMachineClientName),
            _waybackOptions,
            feed.Url,
            _logger,
            cancellationToken);
        if (wayback is not null)
        {
            urls.Add(wayback);
        }

        return urls;
    }
}
