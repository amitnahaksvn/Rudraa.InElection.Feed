using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// New York Times Top Stories API (api.nytimes.com/svc/topstories/v2/{section}.json) - query
/// param api-key. Named ...Api to disambiguate from the already-existing <see cref="RssProviders.NyTimesRssProvider"/>
/// (a plain RSS feed, provider name "NYTimes") - this is the separate official Developer API,
/// provider name "NYTimesAPI". Curl-verified live: an unauthenticated request returns a
/// documented Apigee <c>FailedToResolveAPIKey</c> JSON fault (not a network failure), confirming
/// the endpoint shape; a free key is available from developer.nytimes.com.
/// </summary>
public sealed class NyTimesApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "NYTimesAPI";

    public NyTimesApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NyTimesApiProvider> logger)
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

            var imageUrl = item.TryGetProperty("multimedia", out var multimedia) && multimedia.ValueKind == JsonValueKind.Array
                ? multimedia.EnumerateArray().FirstOrDefault().GetStringOrNull("url")
                : null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = item.GetStringOrNull("section") ?? Name,
                Category = endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("abstract"),
                Url = url,
                OriginalGuid = url,
                Author = item.GetStringOrNull("byline"),
                Language = endpoint.Language,
                ImageUrl = imageUrl,
                PublishedAt = item.GetDateTimeOrNull("published_date"),
                Source = "The New York Times"
            });
        }

        return articles;
    }
}
