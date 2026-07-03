using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// APITube (api.apitube.io/v1/news/everything) - query param <c>api_key</c> (a header
/// <c>X-API-Key</c> also works per its own error message; query param chosen for consistency with
/// the rest of this pipeline). Free plan: limited daily requests - see https://apitube.io/pricing.
/// </summary>
public sealed class ApiTubeProvider : BaseNewsApiProvider
{
    public const string ProviderName = "APITube";

    public ApiTubeProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiTubeProvider> logger)
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
                Content = item.GetStringOrNull("body"),
                Url = url,
                OriginalGuid = item.GetStringOrNull("id"),
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = item.GetDateTimeOrNull("published_at"),
                Source = sourceName ?? "APITube"
            });
        }

        return articles;
    }
}
