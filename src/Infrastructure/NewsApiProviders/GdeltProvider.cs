using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;
using Domain.Enums;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// GDELT Project 2.0 Doc API (api.gdeltproject.org/api/v2/doc/doc) - free, no API key at all
/// (configured with <see cref="ApiAuthType.None"/>). Curl-verified live and unauthenticated;
/// GDELT's own rate-limit message asks for no more than one request every 5 seconds, so this
/// provider's <c>Cron</c> is kept sparse deliberately. https://blog.gdeltproject.org/gdelt-doc-2-0-api-debuts/
/// </summary>
public sealed class GdeltProvider : BaseNewsApiProvider
{
    public const string ProviderName = "GDELT";

    public GdeltProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GdeltProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("articles", out var articlesElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in articlesElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var domain = item.GetStringOrNull("domain");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = domain ?? Name,
                Category = endpoint.Category,
                Title = title,
                Url = url,
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("socialimage"),
                PublishedAt = ParseSeenDate(item.GetStringOrNull("seendate")),
                Source = domain ?? "GDELT"
            });
        }

        return articles;
    }

    /// <summary>GDELT's "seendate" is ISO 8601 basic format ("20260703T151723Z", no dashes/colons) - not a shape DateTimeOffset.Parse accepts without an explicit format string.</summary>
    private static DateTimeOffset? ParseSeenDate(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) &&
        DateTimeOffset.TryParseExact(raw, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
}
