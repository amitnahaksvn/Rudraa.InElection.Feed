using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Webz.io (api.webz.io/newsApiLite) - query param token. Curl-verified live: an unauthenticated
/// request returns a documented "Unknown API token" JSON error (not a network failure),
/// confirming the endpoint shape; a real trial key is free from webz.io's own signup.
/// </summary>
public sealed class WebzIoProvider : BaseNewsApiProvider
{
    public const string ProviderName = "WebzIo";

    public WebzIoProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WebzIoProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("posts", out var postsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in postsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var siteName = item.TryGetProperty("thread", out var thread) ? thread.GetStringOrNull("site") : null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = siteName ?? Name,
                Category = endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("text"),
                Url = url,
                OriginalGuid = item.GetStringOrNull("uuid"),
                Author = item.GetStringOrNull("author"),
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                PublishedAt = item.GetDateTimeOrNull("published"),
                Source = siteName ?? "Webz.io"
            });
        }

        return articles;
    }
}
