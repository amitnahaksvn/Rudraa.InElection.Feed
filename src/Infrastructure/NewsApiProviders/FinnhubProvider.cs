using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Finnhub market news (finnhub.io/api/v1/news) - query param token. Curl-verified live: an
/// unauthenticated request returns a documented <c>{"error":"Invalid API key."}</c> JSON error
/// (not a network failure), confirming the endpoint shape; a free key is available from
/// finnhub.io's own signup. Unlike every other news-API provider here, Finnhub's response root
/// is a bare JSON array, not an object with a named articles/results property - handled by
/// checking <see cref="JsonValueKind.Array"/> directly rather than looking for a wrapper key.
/// </summary>
public sealed class FinnhubProvider : BaseNewsApiProvider
{
    public const string ProviderName = "Finnhub";

    public FinnhubProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FinnhubProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("headline");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var source = item.GetStringOrNull("source");
            var id = item.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.Number
                ? idElement.GetInt64().ToString()
                : null;
            var datetime = item.TryGetProperty("datetime", out var datetimeElement) && datetimeElement.ValueKind == JsonValueKind.Number
                ? DateTimeOffset.FromUnixTimeSeconds(datetimeElement.GetInt64())
                : (DateTimeOffset?)null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = source ?? Name,
                Category = item.GetStringOrNull("category") ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("summary"),
                Url = url,
                OriginalGuid = id,
                Language = endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = datetime,
                Source = source ?? "Finnhub"
            });
        }

        return articles;
    }
}
