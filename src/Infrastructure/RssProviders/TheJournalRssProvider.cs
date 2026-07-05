using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Journal (thejournal.ie, Ireland - English-language) RSS integration - standard
/// WordPress /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Ireland"]:Providers[Name="TheJournal"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TheJournalRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheJournal";
    public const string ClientName = "TheJournalRssClient";

    public TheJournalRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheJournalRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
