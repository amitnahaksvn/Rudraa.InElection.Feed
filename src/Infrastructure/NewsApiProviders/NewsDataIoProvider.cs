using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// NewsData.io (newsdata.io/api/1/news) - query param <c>apikey</c>. Free plan: 200 credits/day,
/// 10 articles/response. https://newsdata.io/pricing
/// </summary>
public sealed class NewsDataIoProvider : BaseNewsApiProvider
{
    public const string ProviderName = "NewsDataIo";

    public NewsDataIoProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NewsDataIoProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var resultsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in resultsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("link");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var sourceName = item.GetStringOrNull("source_id");
            var author = item.GetFirstStringInArrayOrNull("creator");
            var tags = item.GetStringArrayOrEmpty("category");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = sourceName ?? Name,
                Category = tags.FirstOrDefault() ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("description"),
                Content = item.GetStringOrNull("content"),
                Url = url,
                OriginalGuid = item.GetStringOrNull("article_id"),
                Author = author,
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image_url"),
                PublishedAt = item.GetDateTimeOrNull("pubDate"),
                Tags = tags,
                Source = sourceName ?? "NewsData.io"
            });
        }

        return articles;
    }
}
