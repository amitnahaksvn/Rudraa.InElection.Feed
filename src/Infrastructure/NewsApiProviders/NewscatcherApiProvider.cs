using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// NewsCatcher API v3 (v3-api.newscatcherapi.com/api) - header <c>x-api-token</c> (NOT
/// <c>x-api-key</c> - confirmed against the live endpoint's own error message; the "v3-api." host
/// is also easy to miss, the marketing site never states it plainly). Free "Basic" testing plan:
/// very limited trial credits. https://www.newscatcherapi.com/pricing
/// </summary>
public sealed class NewscatcherApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "NewscatcherAPI";

    public NewscatcherApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NewscatcherApiProvider> logger)
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
            var url = item.GetStringOrNull("link");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var domain = item.GetStringOrNull("clean_url") ?? item.GetStringOrNull("domain_url");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = domain ?? Name,
                Category = item.GetStringOrNull("topic") ?? endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("summary") ?? item.GetStringOrNull("excerpt"),
                Url = url,
                OriginalGuid = item.GetStringOrNull("id"),
                Author = item.GetStringOrNull("author"),
                Language = item.GetStringOrNull("language") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("media"),
                PublishedAt = item.GetDateTimeOrNull("published_date"),
                Source = domain ?? "NewsCatcher API"
            });
        }

        return articles;
    }
}
