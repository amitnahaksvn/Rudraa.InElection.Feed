using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// World News API (api.worldnewsapi.com/search-news) - header <c>x-api-key</c> (the provider also
/// accepts a query param, but the header keeps the key out of server/proxy logs, so this provider
/// is configured with <see cref="ApiAuthType.HttpHeader"/>). Free trial: limited total call
/// allowance - see https://worldnewsapi.com/pricing before relying on this in production.
/// </summary>
public sealed class WorldNewsApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "WorldNewsAPI";

    public WorldNewsApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WorldNewsApiProvider> logger)
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

            var author = item.GetFirstStringInArrayOrNull("authors");
            // "id" is a JSON number here (not a string like every other provider's id field), so
            // GetStringOrNull (string-only) won't pick it up - read it as raw text explicitly.
            var id = item.TryGetProperty("id", out var idElement)
                ? idElement.ValueKind switch
                {
                    JsonValueKind.String => idElement.GetString(),
                    JsonValueKind.Number => idElement.GetRawText(),
                    _ => null
                }
                : null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = Name,
                Category = item.GetStringOrNull("category") ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("summary"),
                Content = item.GetStringOrNull("text"),
                Url = url,
                OriginalGuid = id,
                Author = author,
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = item.GetDateTimeOrNull("publish_date"),
                Source = "World News API"
            });
        }

        return articles;
    }
}
