using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// GNews (gnews.io/api/v4/search) - query param <c>apikey</c>. Free plan: 100 requests/day,
/// max 10 articles/request. https://gnews.io/#pricing
/// </summary>
public sealed class GNewsProvider : BaseNewsApiProvider
{
    public const string ProviderName = "GNews";

    public GNewsProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GNewsProvider> logger)
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

            var sourceName = item.TryGetProperty("source", out var source) ? source.GetStringOrNull("name") : null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = sourceName ?? Name,
                Category = endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("description"),
                Content = item.GetStringOrNull("content"),
                Url = url,
                OriginalGuid = url,
                Language = item.GetStringOrNull("lang") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = item.GetDateTimeOrNull("publishedAt"),
                Source = sourceName ?? "GNews"
            });
        }

        return articles;
    }
}
