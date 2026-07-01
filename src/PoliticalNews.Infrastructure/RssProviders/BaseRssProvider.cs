using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Application.Models;
using PoliticalNews.Application.Options;

namespace PoliticalNews.Infrastructure.RssProviders;

/// <summary>
/// Shared download/parse/normalize pipeline for RSS 2.0 feeds. Concrete providers
/// (<see cref="AajTakRssProvider"/> today, ANI/NDTV/PIB/etc. in later phases) only need to
/// supply <see cref="Name"/> and an <see cref="IHttpClientFactory"/> client name - everything
/// else (XML parsing, image extraction, tag extraction, per-feed error isolation) is common.
/// </summary>
public abstract partial class BaseRssProvider : IRssProvider
{
    private static readonly XNamespace Media = "http://search.yahoo.com/mrss/";
    private static readonly XNamespace Content = "http://purl.org/rss/1.0/modules/content/";
    private static readonly XNamespace DublinCore = "http://purl.org/dc/elements/1.1/";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    protected BaseRssProvider(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public abstract string Name { get; }

    /// <summary>Named <see cref="HttpClient"/> registered for this provider in DI.</summary>
    protected abstract string HttpClientName { get; }

    public async Task<IReadOnlyList<FeedFetchResult>> FetchAllFeedsAsync(
        IReadOnlyList<RssFeedOptions> feeds,
        CancellationToken cancellationToken)
    {
        var results = new List<FeedFetchResult>(feeds.Count);

        foreach (var feed in feeds)
        {
            results.Add(await FetchFeedAsync(feed, cancellationToken));
        }

        return results;
    }

    private async Task<FeedFetchResult> FetchFeedAsync(RssFeedOptions feed, CancellationToken cancellationToken)
    {
        var fetchedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        string? rawXml = null;
        int? httpStatusCode = null;

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(feed.Url, cancellationToken);
            httpStatusCode = (int)response.StatusCode;
            response.EnsureSuccessStatusCode();

            rawXml = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = XDocument.Parse(rawXml);

            var articles = new List<NormalizedArticle>();
            foreach (var item in document.Descendants("item"))
            {
                var article = await ParseItemAsync(item, feed, cancellationToken);
                if (article is not null)
                {
                    articles.Add(article);
                }
            }

            return new FeedFetchResult
            {
                FeedName = feed.Name,
                FeedUrl = feed.Url,
                Success = true,
                Articles = articles,
                FetchedAt = fetchedAt,
                HttpStatusCode = httpStatusCode,
                RawXml = rawXml,
                ContentHash = ComputeContentHash(rawXml),
                ProcessingDurationMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Catches everything, including a TaskCanceledException from HttpClient's own
            // per-request Timeout - that is a dead/hanging feed, not our caller asking to stop,
            // and must never crash the host. Only lets an exception through uncaught when our
            // own cancellationToken was actually the one that fired (real shutdown/cancellation).
            _logger.LogError(ex, "Failed to fetch/parse feed {Provider}/{Feed} ({Url})", Name, feed.Name, feed.Url);
            return new FeedFetchResult
            {
                FeedName = feed.Name,
                FeedUrl = feed.Url,
                Success = false,
                Error = ex.Message,
                FetchedAt = fetchedAt,
                HttpStatusCode = httpStatusCode,
                RawXml = rawXml,
                ContentHash = rawXml is not null ? ComputeContentHash(rawXml) : null,
                ProcessingDurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private static string ComputeContentHash(string rawXml) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawXml))).ToLowerInvariant();

    private async Task<NormalizedArticle?> ParseItemAsync(XElement item, RssFeedOptions feed, CancellationToken cancellationToken)
    {
        var title = item.Element("title")?.Value.Trim();
        var link = item.Element("link")?.Value.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
        {
            return null;
        }

        var guid = item.Element("guid")?.Value.Trim();
        var description = item.Element("description")?.Value.Trim();
        var encodedContent = item.Element(Content + "encoded")?.Value.Trim();
        var author = item.Element("author")?.Value.Trim() ?? item.Element(DublinCore + "creator")?.Value.Trim();
        var tags = item.Elements("category").Select(e => e.Value.Trim()).Where(t => t.Length > 0).ToList();
        var publishedAt = ParsePublishDate(item.Element("pubDate")?.Value);
        var imageUrl = ExtractImage(item) ?? await TryExtractOgImageAsync(link, cancellationToken);

        return new NormalizedArticle
        {
            Provider = Name,
            FeedName = feed.Name,
            Category = feed.Category,
            Title = title,
            Summary = StripHtml(description),
            Content = encodedContent ?? description,
            Url = link,
            OriginalGuid = string.IsNullOrWhiteSpace(guid) ? null : guid,
            Author = string.IsNullOrWhiteSpace(author) ? null : author,
            Language = feed.Language,
            ImageUrl = imageUrl,
            PublishedAt = publishedAt,
            Tags = tags,
            Source = feed.Url
        };
    }

    private static string? ExtractImage(XElement item)
    {
        var mediaContent = item.Elements(Media + "content")
            .FirstOrDefault(e => (string?)e.Attribute("url") is not null);
        if (mediaContent is not null)
        {
            return (string?)mediaContent.Attribute("url");
        }

        var mediaThumbnail = item.Element(Media + "thumbnail");
        if (mediaThumbnail is not null)
        {
            var url = (string?)mediaThumbnail.Attribute("url");
            if (!string.IsNullOrWhiteSpace(url))
            {
                return url;
            }
        }

        var enclosure = item.Elements("enclosure")
            .FirstOrDefault(e => ((string?)e.Attribute("type"))?.StartsWith("image", StringComparison.OrdinalIgnoreCase) == true);
        if (enclosure is not null)
        {
            return (string?)enclosure.Attribute("url");
        }

        return null;
    }

    private async Task<string?> TryExtractOgImageAsync(string articleUrl, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(articleUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var match = OgImageRegex().Match(html);
            return match.Success ? match.Groups["url"].Value : null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "og:image fallback lookup failed for {Url}", articleUrl);
            return null;
        }
    }

    private static DateTimeOffset? ParsePublishDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static string? StripHtml(string? html) =>
        string.IsNullOrWhiteSpace(html) ? null : HtmlTagRegex().Replace(html, string.Empty).Trim();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(
        """<meta[^>]+property=["']og:image["'][^>]+content=["'](?<url>[^"']+)["']""",
        RegexOptions.IgnoreCase)]
    private static partial Regex OgImageRegex();
}
