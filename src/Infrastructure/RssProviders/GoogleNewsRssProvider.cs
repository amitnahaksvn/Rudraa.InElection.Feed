using System.Net;
using Microsoft.Extensions.Logging;
using Application.Options;

namespace Infrastructure.RssProviders;

/// <summary>
/// Google News RSS integration - unlike every other provider, a configured feed's
/// <see cref="RssFeedOptions.Url"/> is not a literal feed URL but a plain-text search topic
/// (e.g. "India politics"); <see cref="ResolveFeedUrl"/> builds the actual
/// <c>news.google.com/rss/search</c> URL from it. This means adding a new topic is purely a
/// configuration change - one new entry in NewsCrawler:Providers[Name="GoogleNews"]:Feeds with the
/// topic text as Url - no code or URL-construction knowledge required.
///
/// Known limitations, both accepted deliberately rather than worked around:
/// (1) Google's own feed &lt;copyright&gt; notice restricts use to "rendering...within a personal
/// feed reader for personal, non-commercial use" - wiring this into a persisted, served crawler
/// is arguably outside that, an explicit risk accepted for this app.
/// (2) Each article &lt;link&gt; is an opaque news.google.com/rss/articles/{token} redirect that
/// does not resolve to the real publisher URL via a simple HTTP redirect (Google resolves it
/// client-side via JS); reliably decoding it would require an unofficial reverse-engineered
/// scheme or a headless browser, both judged too fragile to depend on. Articles are stored under
/// their Google-hosted link as-is, so the same story already ingested from a direct publisher
/// feed will not dedupe against its Google News copy - they are intentionally separate documents.
///
/// <see cref="ResolveFeedUrl"/> derives the <c>hl</c>/<c>ceid</c> query parameters from the feed's
/// own <see cref="RssFeedOptions.Language"/> (already required on every feed for article tagging,
/// so this reuses it rather than adding a new property) instead of hardcoding English - needed for
/// state-level regional-language searches (e.g. Language "te" -> hl=te&amp;ceid=IN:te for a Telugu
/// query), verified against Google News' own accepted <c>hl</c> values. English still maps to
/// "en-IN" specifically (not bare "en"), matching Google News' own convention for the English-India
/// edition.
/// </summary>
public sealed class GoogleNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "GoogleNews";
    public const string ClientName = "GoogleNewsRssClient";

    public GoogleNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<GoogleNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;

    protected override string ResolveFeedUrl(RssFeedOptions feed)
    {
        var language = string.IsNullOrWhiteSpace(feed.Language) ? "en" : feed.Language;
        var hl = language == "en" ? "en-IN" : language;
        return $"https://news.google.com/rss/search?q={WebUtility.UrlEncode(feed.Url)}&hl={hl}&gl=IN&ceid=IN:{language}";
    }
}
