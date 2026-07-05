using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Alpha Vantage News &amp; Sentiment (alphavantage.co/query?function=NEWS_SENTIMENT) - query
/// param apikey. Curl-verified live and returning real, current financial news even against the
/// public "demo" key (Alpha Vantage's own documented special case for the AAPL example ticker,
/// same "demo key works for verification, get a real one for production" situation as FEC's
/// DEMO_KEY elsewhere in this codebase).
/// </summary>
public sealed class AlphaVantageProvider : BaseNewsApiProvider
{
    public const string ProviderName = "AlphaVantage";

    public AlphaVantageProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AlphaVantageProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("feed", out var feedElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in feedElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var source = item.GetStringOrNull("source");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = source ?? Name,
                Category = endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("summary"),
                Url = url,
                OriginalGuid = url,
                Author = item.GetFirstStringInArrayOrNull("authors"),
                Language = endpoint.Language,
                ImageUrl = item.GetStringOrNull("banner_image"),
                PublishedAt = ParseTimePublished(item.GetStringOrNull("time_published")),
                Source = source ?? "Alpha Vantage"
            });
        }

        return articles;
    }

    /// <summary>Alpha Vantage's "time_published" is ISO 8601 basic format ("20260705T180311", no dashes/colons) - same shape GDELT's "seendate" uses, not a shape DateTimeOffset.Parse accepts without an explicit format string.</summary>
    private static DateTimeOffset? ParseTimePublished(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) &&
        DateTimeOffset.TryParseExact(raw, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
}
