using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Models;
using Application.Options;
using Infrastructure.DependencyInjection;

namespace Infrastructure.RssProviders;

/// <summary>
/// Press Information Bureau (pib.gov.in) RSS integration - feeds live under
/// RssMain.aspx?ModId={category}&amp;lang={language}, discovered by probing the ASP.NET
/// WebForms query-parameter scheme directly (ModId=6 press releases, ModId=8 photo features,
/// ModId=9 media invitations; lang=1 English, lang=2 Hindi, lang=3 Urdu - lang values above 3 all
/// silently fall back to Hindi content, so there is no genuine "regional" language variant beyond
/// these three). Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="PIB"]:Feeds, never hardcoded here.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> - production logs show a consistent
/// 30-second connection timeout against this app's Azure outbound IP (confirmed live the same
/// request resolves in well under a second from a non-Azure network), the same
/// connection-hang-not-a-fast-403 signature already documented for MPInfo/NDMA, not the
/// WAF-UA-detection reason PIB's BrowserUserAgent registration was originally added for.
/// </summary>
public sealed class PibRssProvider : BaseRssProvider
{
    public const string ProviderName = "PIB";
    public const string ClientName = "PibRssClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<PibRssProvider> _logger;

    public PibRssProvider(IHttpClientFactory httpClientFactory, ILogger<PibRssProvider> logger, IOptions<WaybackMachineOptions> waybackOptions)
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
