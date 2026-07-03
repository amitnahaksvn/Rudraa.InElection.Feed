using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// TheNewsAPI (api.thenewsapi.com/v1/news/all) - query param <c>api_token</c>. Free plan:
/// 100 requests/day, 3 articles/response. https://www.thenewsapi.com/pricing
/// </summary>
public sealed class TheNewsApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "TheNewsAPI";

    public TheNewsApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TheNewsApiProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var dataElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in dataElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var source = item.GetStringOrNull("source");
            var tags = item.GetStringArrayOrEmpty("categories");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = source ?? Name,
                Category = tags.FirstOrDefault() ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("description") ?? item.GetStringOrNull("snippet"),
                Content = item.GetStringOrNull("snippet"),
                Url = url,
                OriginalGuid = item.GetStringOrNull("uuid"),
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image_url"),
                PublishedAt = item.GetDateTimeOrNull("published_at"),
                Tags = tags,
                Source = source ?? "TheNewsAPI"
            });
        }

        return articles;
    }
}
