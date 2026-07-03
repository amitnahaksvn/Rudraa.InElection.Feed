using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Mediastack (api.mediastack.com/v1/news) - query param <c>access_key</c>. Free plan: HTTP only
/// (no HTTPS - see <c>BaseUrl</c> in config), 100 requests/MONTH (not per day), so this provider's
/// configured <c>Cron</c> is deliberately sparse. https://mediastack.com/product
/// </summary>
public sealed class MediastackProvider : BaseNewsApiProvider
{
    public const string ProviderName = "Mediastack";

    public MediastackProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<MediastackProvider> logger)
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

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = source ?? Name,
                Category = item.GetStringOrNull("category") ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("description"),
                Url = url,
                OriginalGuid = url,
                Author = item.GetStringOrNull("author"),
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = item.GetDateTimeOrNull("published_at"),
                Source = source ?? "Mediastack"
            });
        }

        return articles;
    }
}
