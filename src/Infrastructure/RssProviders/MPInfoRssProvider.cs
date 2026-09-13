using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// MPinfo (mpinfo.org) RSS integration - Madhya Pradesh's own Public Relations Department feeds
/// (Hindi/English general news, CM news, cabinet decisions). Feed URLs live entirely in
/// configuration under NewsCrawler:Providers[Name="MPInfo"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> - confirmed live that mpinfo.org's own
/// server blocks every cloud IP range tested (Azure App Service, Cloudflare Workers, GitHub
/// Actions runners: all three hang/time out on both HTTP and HTTPS), while the Wayback Machine's
/// own crawler successfully fetched this exact feed as recently as 2026-02-14 - see
/// <see cref="WaybackMachineFeedResolver"/>'s own doc comment for the full mechanism and why this
/// works where a self-hosted proxy (tried and confirmed also blocked) didn't.
/// </summary>
public sealed class MPInfoRssProvider : BaseRssProvider
{
    public const string ProviderName = "MPInfo";
    public const string ClientName = "MPInfoRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<MPInfoRssProvider> _logger;

    public MPInfoRssProvider(IHttpClientFactory httpClientFactory, ILogger<MPInfoRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
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
