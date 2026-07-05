using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// NZZ (nzz.ch, Switzerland - German-language) RSS integration - the requested nzz.ch/rss
/// returns the plain homepage HTML, not a feed; the real one is nzz.ch/recent.rss (discovered by
/// testing NZZ's other known *.rss suffix convention, e.g. international.rss). Feed URL lives
/// entirely in configuration under
/// NewsCrawler:Countries[Name="Switzerland"]:Providers[Name="NZZ"]:Feeds, never hardcoded here.
/// </summary>
public sealed class NzzRssProvider : BaseRssProvider
{
    public const string ProviderName = "NZZ";
    public const string ClientName = "NzzRssClient";

    public NzzRssProvider(IHttpClientFactory httpClientFactory, ILogger<NzzRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
