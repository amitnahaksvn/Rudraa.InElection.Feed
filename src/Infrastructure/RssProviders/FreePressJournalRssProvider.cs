using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Free Press Journal (freepressjournal.in) - served as Atom 1.0 from QuintType's own CDN
/// (prod-qt-images.s3.amazonaws.com/production/freepressjournal/feed.xml), not RSS 2.0 - see
/// <see cref="BaseAtomRssProvider"/>'s own doc comment. Feed URL lives entirely in configuration
/// under NewsCrawler:Providers[Name="FreePressJournal"]:Feeds, never hardcoded here.
/// </summary>
public sealed class FreePressJournalRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "FreePressJournal";
    public const string ClientName = "FreePressJournalRssClient";

    public FreePressJournalRssProvider(IHttpClientFactory httpClientFactory, ILogger<FreePressJournalRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
