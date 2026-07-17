using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Associated Press Content API (api.ap.org/media/v/content/search) - query param apikey.
/// Curl-verified live: an unauthenticated request returns a documented
/// <c>{"error":{"code":2000,"message":"Supply a valid apikey"}}</c> JSON error (not a network
/// failure), confirming the endpoint shape; there is no free tier (per the source list this
/// provider was added from) - a key requires a paid AP Content API subscription via
/// developer.ap.org. The response's <c>data.items[].item</c> nesting is AP's own documented
/// shape; no real key was available while this was built, so - a "best-effort, confirm once
/// enabled" caveat - field names should be re-checked against a live response before relying on
/// this in production.
/// </summary>
public sealed class ApContentApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "APContentAPI";

    public ApContentApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApContentApiProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("items", out var itemsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var wrapper in itemsElement.EnumerateArray())
        {
            if (!wrapper.TryGetProperty("item", out var item))
            {
                continue;
            }

            var title = item.GetStringOrNull("headline");
            var uri = item.GetStringOrNull("uri");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(uri))
            {
                continue;
            }

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = Name,
                Category = endpoint.Category,
                Title = title,
                Url = uri,
                OriginalGuid = item.TryGetProperty("altids", out var altIds) ? altIds.GetStringOrNull("itemid") : null,
                Language = endpoint.Language,
                PublishedAt = item.GetDateTimeOrNull("versioncreated"),
                Source = "Associated Press"
            });
        }

        return articles;
    }
}
