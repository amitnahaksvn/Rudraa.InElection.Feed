using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// The Guardian Open Platform (content.guardianapis.com/search) - query param <c>api-key</c>.
/// Free "Developer" tier: 12 calls/second, 5000 calls/day, non-commercial use.
/// <c>show-fields=thumbnail,trailText,byline</c> is required in every endpoint's configured query
/// parameters to get an image/summary/author at all - Guardian omits them from the bare response.
/// https://open-platform.theguardian.com/access/
/// </summary>
public sealed class GuardianProvider : BaseNewsApiProvider
{
    public const string ProviderName = "Guardian";

    public GuardianProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GuardianProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("response", out var response) ||
            !response.TryGetProperty("results", out var resultsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in resultsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("webTitle");
            var url = item.GetStringOrNull("webUrl");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var fields = item.TryGetProperty("fields", out var fieldsElement) ? fieldsElement : default;
            var summary = fields.ValueKind == JsonValueKind.Object ? fields.GetStringOrNull("trailText") : null;
            var author = fields.ValueKind == JsonValueKind.Object ? fields.GetStringOrNull("byline") : null;
            var image = fields.ValueKind == JsonValueKind.Object ? fields.GetStringOrNull("thumbnail") : null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = item.GetStringOrNull("sectionName") ?? Name,
                Category = item.GetStringOrNull("sectionName") ?? endpoint.Category,
                Title = title,
                Summary = summary,
                Url = url,
                OriginalGuid = item.GetStringOrNull("id"),
                Author = author,
                Language = endpoint.Language,
                ImageUrl = image,
                PublishedAt = item.GetDateTimeOrNull("webPublicationDate"),
                Source = "The Guardian"
            });
        }

        return articles;
    }
}
