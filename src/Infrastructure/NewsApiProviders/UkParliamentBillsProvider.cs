using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;
using Domain.Enums;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// UK Parliament Bills API (bills-api.parliament.uk/api/v1/Bills) - curl-verified fully public,
/// no API key or auth of any kind (configured with <see cref="ApiAuthType.None"/>, same as
/// GDELT). Each "article" here is a bill's current status, not a written news story - title is
/// the bill's short title, content is its current stage description, so a bill reaching a new
/// reading/stage shows up as a distinct, dated update every time this endpoint is polled.
/// </summary>
public sealed class UkParliamentBillsProvider : BaseNewsApiProvider
{
    public const string ProviderName = "UKParliamentBills";

    public UkParliamentBillsProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<UkParliamentBillsProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("items", out var itemsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in itemsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("shortTitle");
            var billId = item.TryGetProperty("billId", out var billIdElement) && billIdElement.ValueKind == JsonValueKind.Number
                ? billIdElement.GetInt32().ToString()
                : null;
            if (string.IsNullOrWhiteSpace(title) || billId is null)
            {
                continue;
            }

            var currentHouse = item.GetStringOrNull("currentHouse");
            var stageDescription = item.TryGetProperty("currentStage", out var stage)
                ? stage.GetStringOrNull("description")
                : null;

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = Name,
                Category = endpoint.Category,
                Title = title,
                Summary = stageDescription is not null ? $"{stageDescription} ({currentHouse})" : currentHouse,
                Content = stageDescription,
                Url = $"https://bills.parliament.uk/bills/{billId}",
                OriginalGuid = billId,
                Language = endpoint.Language,
                PublishedAt = item.GetDateTimeOrNull("lastUpdate"),
                Source = "UK Parliament"
            });
        }

        return articles;
    }
}
