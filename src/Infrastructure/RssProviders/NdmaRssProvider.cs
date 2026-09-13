using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// National Disaster Management Authority (ndma.gov.in) RSS integration - the bare /rss.xml
/// endpoint. Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="NDMA"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> - confirmed live that ndma.gov.in's own
/// server blocks every cloud IP range tested (Azure App Service, Cloudflare Workers, GitHub
/// Actions runners: all three hang/time out on both HTTP and HTTPS, identically to MPInfo - see
/// its own doc comment for the full diagnosis), while the Wayback Machine's own crawler is
/// confirmed able to reach ndma.gov.in's homepage - see <see cref="WaybackMachineFeedResolver"/>'s
/// own doc comment for the full mechanism. Note: unlike MPInfo, archive.org had never crawled this
/// exact rss.xml URL as of when this was written, so the on-demand "Save Page Now" capture (which
/// needs <see cref="WaybackMachineOptions"/> credentials) is what actually produces data here -
/// the passive already-archived-snapshot fallback alone has nothing to fall back to for this feed.
/// </summary>
public sealed class NdmaRssProvider : BaseRssProvider
{
    public const string ProviderName = "NDMA";
    public const string ClientName = "NdmaRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<NdmaRssProvider> _logger;

    public NdmaRssProvider(IHttpClientFactory httpClientFactory, ILogger<NdmaRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
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
