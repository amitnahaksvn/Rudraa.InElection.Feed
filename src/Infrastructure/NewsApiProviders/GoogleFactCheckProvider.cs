using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Google Fact Check Tools API (factchecktools.googleapis.com/v1alpha1/claims:search) - query
/// param key. Curl-verified live: an unauthenticated request returns a documented
/// PERMISSION_DENIED JSON error naming the exact auth requirement (not a network failure),
/// confirming the endpoint shape; a real key is free from a Google Cloud Console project (enable
/// the "Fact Check Tools API"). A claim can carry more than one claimReview (different
/// fact-checkers reviewing the same claim) - only the first is used, same "first is good enough,
/// don't fan out one claim into N articles" reasoning as every other provider's
/// GetFirstStringInArrayOrNull use.
/// </summary>
public sealed class GoogleFactCheckProvider : BaseNewsApiProvider
{
    public const string ProviderName = "GoogleFactCheck";

    public GoogleFactCheckProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GoogleFactCheckProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("claims", out var claimsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var claim in claimsElement.EnumerateArray())
        {
            var claimText = claim.GetStringOrNull("text");
            if (string.IsNullOrWhiteSpace(claimText) ||
                !claim.TryGetProperty("claimReview", out var reviewsElement) ||
                reviewsElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var review = reviewsElement.EnumerateArray().FirstOrDefault();
            var url = review.GetStringOrNull("url");
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var publisherName = review.TryGetProperty("publisher", out var publisher) ? publisher.GetStringOrNull("name") : null;
            var rating = review.GetStringOrNull("textualRating");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = publisherName ?? Name,
                Category = endpoint.Category,
                Title = review.GetStringOrNull("title") ?? claimText,
                Summary = rating is not null ? $"Claim: \"{claimText}\" - Rating: {rating}" : claimText,
                Url = url,
                OriginalGuid = url,
                Author = claim.GetStringOrNull("claimant"),
                Language = endpoint.Language,
                PublishedAt = review.GetDateTimeOrNull("reviewDate") ?? claim.GetDateTimeOrNull("claimDate"),
                Source = publisherName ?? "Google Fact Check"
            });
        }

        return articles;
    }
}
