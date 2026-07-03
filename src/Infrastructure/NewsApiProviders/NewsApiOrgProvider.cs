using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// NewsAPI.org (newsapi.org/v2/everything) - query param <c>apiKey</c>. Free "Developer" plan:
/// 100 requests/day, articles delayed ~24h, non-commercial use only. https://newsapi.org/pricing
/// </summary>
public sealed class NewsApiOrgProvider : BaseNewsApiProvider
{
    public const string ProviderName = "NewsApiOrg";

    public NewsApiOrgProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NewsApiOrgProvider> logger)
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
                Author = item.GetStringOrNull("author"),
                Language = endpoint.Language,
                ImageUrl = item.GetStringOrNull("urlToImage"),
                PublishedAt = item.GetDateTimeOrNull("publishedAt"),
                Source = sourceName ?? "NewsAPI.org"
            });
        }

        return articles;
    }
}
