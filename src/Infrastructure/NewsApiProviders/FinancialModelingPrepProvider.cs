using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Financial Modeling Prep general market news (financialmodelingprep.com/stable/general-news) -
/// query param apikey. Curl-verified live: an unauthenticated request returns a documented
/// <c>{"Error Message":"Invalid API KEY..."}</c> JSON error (not a network failure), confirming
/// the endpoint shape; a free key is available from financialmodelingprep.com's own signup. Same
/// bare-JSON-array response shape as <see cref="FinnhubProvider"/> - handled the same way.
/// </summary>
public sealed class FinancialModelingPrepProvider : BaseNewsApiProvider
{
    public const string ProviderName = "FinancialModelingPrep";

    public FinancialModelingPrepProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FinancialModelingPrepProvider> logger)
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
            var title = item.GetStringOrNull("title");
            var url = item.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var site = item.GetStringOrNull("site");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = site ?? Name,
                Category = endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("text"),
                Url = url,
                OriginalGuid = url,
                Language = endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = item.GetDateTimeOrNull("publishedDate"),
                Source = site ?? "Financial Modeling Prep"
            });
        }

        return articles;
    }
}
