using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// GOV.UK (gov.uk, UK - English-language government announcements) integration - served as Atom
/// 1.0, not RSS 2.0, so this extends <see cref="BaseAtomRssProvider"/> rather than
/// <see cref="BaseRssProvider"/>, same reasoning as Naharnet/Roya News/Stuff. The feed is
/// GOV.UK's own site-search-as-feed endpoint,
/// gov.uk/search/news-and-communications.atom?keywords={term} - "election" is used as the
/// keyword to keep this feed relevant to this app's domain rather than pulling every government
/// announcement (import tariffs, park opening hours, etc.). No image tags in entries, so images
/// come from the og:image HTML fallback. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="United Kingdom"]:Providers[Name="GOVUKNews"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class GovUkNewsRssProvider : BaseAtomRssProvider
{
    public const string ProviderName = "GOVUKNews";
    public const string ClientName = "GovUkNewsRssClient";

    public GovUkNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<GovUkNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
