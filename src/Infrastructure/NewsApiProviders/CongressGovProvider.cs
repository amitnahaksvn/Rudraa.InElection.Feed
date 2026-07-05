using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Congress.gov API (api.congress.gov/v3) - query param api_key. Curl-verified live: an
/// unauthenticated request returns a documented API_KEY_MISSING JSON error (not a network
/// failure), confirming the endpoint shape; a real key is free from
/// https://api.congress.gov/sign-up/. Each "article" here is a bill's latest action, not a
/// written story - title is the bill's own title, content is its most recent recorded action
/// text/date, mirroring <see cref="UkParliamentBillsProvider"/>'s same "status update, not a
/// story" shape for the UK's equivalent.
/// </summary>
public sealed class CongressGovProvider : BaseNewsApiProvider
{
    public const string ProviderName = "CongressGov";

    public CongressGovProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CongressGovProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("bills", out var billsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in billsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title");
            var congress = item.TryGetProperty("congress", out var congressElement) && congressElement.ValueKind == JsonValueKind.Number
                ? congressElement.GetInt32().ToString()
                : null;
            var number = item.GetStringOrNull("number");
            var billType = item.GetStringOrNull("type");
            if (string.IsNullOrWhiteSpace(title) || congress is null || number is null || billType is null)
            {
                continue;
            }

            var latestAction = item.TryGetProperty("latestAction", out var actionElement)
                ? actionElement.GetStringOrNull("text")
                : null;
            var originChamber = item.GetStringOrNull("originChamber");
            var billIdentifier = $"{billType}{number}-{congress}";

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = Name,
                Category = endpoint.Category,
                Title = title,
                Summary = latestAction,
                Content = latestAction,
                Url = $"https://www.congress.gov/bill/{congress}th-congress/{originChamber?.ToLowerInvariant() ?? "house"}-bill/{number}",
                OriginalGuid = billIdentifier,
                Language = endpoint.Language,
                PublishedAt = item.GetDateTimeOrNull("updateDate"),
                Source = "Congress.gov"
            });
        }

        return articles;
    }
}
