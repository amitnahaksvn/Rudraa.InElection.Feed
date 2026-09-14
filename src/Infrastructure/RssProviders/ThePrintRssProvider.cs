using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// ThePrint (theprint.in) RSS integration - standard WordPress category feeds
/// (/category/{name}/feed/; the bare /feed/ itself returns no items, so only category feeds are
/// used). Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="ThePrint"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> - production logs show occasional
/// Cloudflare "Just a moment..." JS-challenge pages (HTTP 200, but an interstitial HTML shell, not
/// the feed - surfaces as an XmlException since the shell's malformed head/meta tags don't parse)
/// from this app's Azure outbound IP - confirmed live the exact same request returns the real feed
/// immediately, no challenge at all, from a non-Azure network with either UA. Same IP-reputation
/// root cause as every other Wayback-routed provider in this file, just a different failure
/// signature (a JS challenge instead of a 403/timeout/DNS failure/404) since Cloudflare's bot
/// scoring - not the origin server - is what's blocking this app's IP here.
/// </summary>
public sealed class ThePrintRssProvider : BaseRssProvider
{
    public const string ProviderName = "ThePrint";
    public const string ClientName = "ThePrintRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<ThePrintRssProvider> _logger;

    public ThePrintRssProvider(IHttpClientFactory httpClientFactory, ILogger<ThePrintRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
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
