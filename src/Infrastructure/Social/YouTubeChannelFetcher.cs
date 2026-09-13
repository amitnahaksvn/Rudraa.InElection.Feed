using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Models;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DependencyInjection;
using Infrastructure.RssProviders;

namespace Infrastructure.Social;

/// <summary>
/// Fetches one YouTube channel's Atom feed (<c>youtube.com/feeds/videos.xml?channel_id=...</c>)
/// for a Mongo-driven <see cref="SocialMediaSource"/> - the DB-driven counterpart to the
/// file-configured <see cref="YouTubeRssProvider"/>. Deliberately a separate class rather than
/// reusing <see cref="YouTubeRssProvider"/> directly: that one's per-entry parser is tightly typed
/// to <c>RssFeedOptions</c> (name/category/language from a config feed entry), whereas this reads
/// the same fields off a <see cref="SocialMediaSource"/> document instead - same Atom shape
/// (entry/published/id/link/media:group), same reasoning for why it isn't RSS 2.0 either, just a
/// different config source. Reuses <see cref="YouTubeRssProvider.ClientName"/>'s already-registered
/// HttpClient rather than adding a second one for the exact same target domain.
///
/// Routed through <see cref="WaybackMachineFeedResolver"/> after 113 unresolved HTTP 404s
/// accumulated in production (both the Modi and BJP channels, identically) - confirmed live from
/// GitHub Actions (not Azure) that both feed URLs return HTTP 200 with real, correct content
/// (channel titles match), so this is the same edge/network block against this app's specific
/// Azure outbound IP already documented for News18/Organiser/IndianExpress/MPInfo/NDMA, just a new
/// failure-signature variant (404 instead of 403/timeout/DNS failure).
/// </summary>
public sealed class YouTubeChannelFetcher : ISocialPlatformFetcher
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace Media = "http://search.yahoo.com/mrss/";
    private static readonly XNamespace Yt = "http://www.youtube.com/xml/schemas/2015";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WaybackMachineOptions _waybackOptions;
    private readonly ILogger<YouTubeChannelFetcher> _logger;

    public YouTubeChannelFetcher(IHttpClientFactory httpClientFactory, ILogger<YouTubeChannelFetcher> logger, IOptions<WaybackMachineOptions> waybackOptions)
    {
        _httpClientFactory = httpClientFactory;
        _waybackOptions = waybackOptions.Value;
        _logger = logger;
    }

    public SocialPlatform Platform => SocialPlatform.YouTube;

    public async Task<IReadOnlyList<NormalizedArticle>> FetchAsync(SocialMediaSource source, CancellationToken cancellationToken)
    {
        var realFeedUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={Uri.EscapeDataString(source.Identifier)}";
        var feedUrl = await WaybackMachineFeedResolver.ResolveAsync(
            _httpClientFactory.CreateClient(InfrastructureServiceCollectionExtensions.WaybackMachineClientName),
            _waybackOptions,
            realFeedUrl,
            _logger,
            cancellationToken);

        var client = _httpClientFactory.CreateClient(YouTubeRssProvider.ClientName);
        using var response = await client.GetAsync(feedUrl, cancellationToken);
        // Body read before the status check throws, not after, so a non-2xx response's body is
        // still captured (via the exception's Data / logs) for diagnostics instead of being
        // discarded - same reasoning as every other fetch in this codebase.
        var rawXml = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        var document = XDocument.Parse(rawXml);

        return document.Descendants(Atom + "entry")
            .Select(entry => ParseEntry(entry, source, feedUrl))
            .Where(article => article is not null)
            .Select(article => article!)
            .ToList();
    }

    private static NormalizedArticle? ParseEntry(XElement entry, SocialMediaSource source, string feedUrl)
    {
        var title = entry.Element(Atom + "title")?.Value.Trim();
        var link = entry.Elements(Atom + "link")
            .FirstOrDefault(e => (string?)e.Attribute("rel") is null or "alternate")?
            .Attribute("href")?.Value;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
        {
            return null;
        }

        var videoId = entry.Element(Yt + "videoId")?.Value.Trim();
        var author = entry.Element(Atom + "author")?.Element(Atom + "name")?.Value.Trim();
        // .ToUniversalTime() so PublishedAt is consistently UTC (Offset=00:00). AssumeUniversal so
        // an entry with no offset at all parses to UTC deterministically rather than silently
        // assuming this host machine's own local time zone - see BaseRssProvider.ParsePublishDate's
        // own doc comment for why that distinction matters here.
        var publishedAt = DateTimeOffset.TryParse(entry.Element(Atom + "published")?.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.ToUniversalTime()
            : (DateTimeOffset?)null;

        var mediaGroup = entry.Element(Media + "group");
        var description = mediaGroup?.Element(Media + "description")?.Value.Trim();
        var thumbnail = mediaGroup?.Element(Media + "thumbnail")?.Attribute("url")?.Value;

        return new NormalizedArticle
        {
            Provider = source.Platform.ToString(),
            FeedName = source.Name,
            Category = source.Category,
            Title = title,
            Summary = Truncate(description, 500),
            Content = description,
            Url = link,
            OriginalGuid = videoId,
            Author = author,
            Language = source.Language,
            Country = source.Country,
            ImageUrl = thumbnail,
            PublishedAt = publishedAt,
            Tags = ["video"],
            Source = feedUrl
        };
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];
}
