using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NITI Aayog (niti.gov.in, English-language policy think tank) RSS integration -
/// niti.gov.in/rss.xml - a Drupal-generated feed that declares a text/html Content-Type despite
/// being real, well-formed RSS 2.0 (same harmless mislabel already seen with Kathmandu Post -
/// BaseRssProvider never inspects Content-Type before parsing); 10 recent, genuinely current
/// items (reports/launches) at verification time. Corrects an earlier finding elsewhere in this
/// file that NITI Aayog's own website had no discoverable RSS - a re-check turned up a real one
/// this time, either because the site changed or the earlier pass didn't try this exact path.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="India"]:Providers[Name="NITIAayog"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NitiAayogRssProvider : BaseRssProvider
{
    public const string ProviderName = "NITIAayog";
    public const string ClientName = "NitiAayogRssClient";

    public NitiAayogRssProvider(IHttpClientFactory httpClientFactory, ILogger<NitiAayogRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
