using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Currents API (api.currentsapi.services/v1/latest-news) - query param <c>apiKey</c>. Free plan:
/// 20 requests/min, 600 requests/day. https://currentsapi.services/en/pricing
/// </summary>
public sealed class CurrentsApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "CurrentsAPI";

    public CurrentsApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CurrentsApiProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("news", out var newsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in newsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var tags = item.GetStringArrayOrEmpty("category");
            // Currents API returns the literal string "None" (not a JSON null/absent key) when an
            // article has no image - must be filtered explicitly or it gets stored as a fake URL.
            var image = item.GetStringOrNull("image");
            if (string.Equals(image, "None", StringComparison.OrdinalIgnoreCase))
            {
                image = null;
            }

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = Name,
                Category = tags.FirstOrDefault() ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("description"),
                Url = url,
                OriginalGuid = item.GetStringOrNull("id"),
                Author = item.GetStringOrNull("author"),
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = image,
                PublishedAt = item.GetDateTimeOrNull("published"),
                Tags = tags,
                Source = "Currents API"
            });
        }

        return articles;
    }
}
