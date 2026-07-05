using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// ProPublica Congress API (api.propublica.org/congress/v1) - header X-API-Key. Named
/// ...Congress to disambiguate from the already-existing <see cref="RssProviders.ProPublicaRssProvider"/>
/// (a plain RSS feed, provider name "ProPublica") - this is ProPublica's separate legislative
/// datastore API, provider name "ProPublicaCongress", same "not a written story, a bill's latest
/// action" shape as <see cref="UkParliamentBillsProvider"/>/<see cref="CongressGovProvider"/>.
/// Curl-verified live: an unauthenticated request returns a documented
/// <c>{"message":"Unauthorized"}</c> JSON error (not a network failure), confirming the endpoint
/// shape; a free key is available from propublica.org/datastore/api.
/// </summary>
public sealed class ProPublicaCongressApiProvider : BaseNewsApiProvider
{
    public const string ProviderName = "ProPublicaCongress";

    public ProPublicaCongressApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ProPublicaCongressApiProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var resultsElement) ||
            resultsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var result in resultsElement.EnumerateArray())
        {
            if (!result.TryGetProperty("bills", out var billsElement) || billsElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var bill in billsElement.EnumerateArray())
            {
                var title = bill.GetStringOrNull("short_title") ?? bill.GetStringOrNull("title");
                var url = bill.GetStringOrNull("congressdotgov_url");
                var billId = bill.GetStringOrNull("bill_id");
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url) || billId is null)
                {
                    continue;
                }

                articles.Add(new NormalizedArticle
                {
                    Provider = Name,
                    FeedName = Name,
                    Category = endpoint.Category,
                    Title = title,
                    Summary = bill.GetStringOrNull("latest_major_action"),
                    Content = bill.GetStringOrNull("latest_major_action"),
                    Url = url,
                    OriginalGuid = billId,
                    Language = endpoint.Language,
                    PublishedAt = bill.GetDateTimeOrNull("latest_major_action_date") ?? bill.GetDateTimeOrNull("introduced_date"),
                    Source = "ProPublica Congress API"
                });
            }
        }

        return articles;
    }
}
