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
/// Routed through <see cref="WaybackMachineFeedResolver"/> - indianexpress.com's CloudFront
/// distribution returns an instant (single-digit-millisecond) HTTP 403 "Request blocked" page to
/// every request from this app's Azure outbound IP, on every one of its 21 feeds, identically with
/// the already-configured <c>BrowserUserAgent</c> - confirmed live that the exact same request
/// succeeds (HTTP 200) from a non-Azure network, so this is an IP-reputation/rate block at
/// CloudFront's edge, not a UA or content issue. Same root cause category as
/// <see cref="MPInfoRssProvider"/>/<see cref="NdmaRssProvider"/> (an edge/network block against
/// this app's specific outbound IP) even though the failure signature differs (instant 403 here vs.
/// their connection hangs), so the same fix applies - see
/// <see cref="WaybackMachineFeedResolver"/>'s own doc comment for the full mechanism. Unlike those
/// two single-feed low-volume providers, this one has 21 feeds on a 30-minute cron, so an on-demand
/// capture taking up to ~30s per feed in the worst case still fits comfortably within that cadence.
/// </summary>
public sealed class IndianExpressRssProvider : BaseRssProvider
{
    public const string ProviderName = "IndianExpress";
    public const string ClientName = "IndianExpressRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<IndianExpressRssProvider> _logger;

    public IndianExpressRssProvider(IHttpClientFactory httpClientFactory, ILogger<IndianExpressRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
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
