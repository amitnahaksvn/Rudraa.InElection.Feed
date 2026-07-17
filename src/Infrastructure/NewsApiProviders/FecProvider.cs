using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// FEC/Federal Election Commission API (api.open.fec.gov/v1) - query param api_key. Curl-verified
/// live and returning real data even against the public rate-limited DEMO_KEY (real production
/// use needs a free key from https://api.data.gov/signup/, same "the free/demo tier works for
/// verification, swap in a real key before relying on it" situation as most providers here). Each
/// "article" here is a candidate filing record, not a written story - there is no natural
/// title/body split, so the candidate's name plus office/party stands in for both.
/// </summary>
public sealed class FecProvider : BaseNewsApiProvider
{
    public const string ProviderName = "FEC";

    public FecProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FecProvider> logger)
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
            var name = item.GetStringOrNull("name");
            var candidateId = item.GetStringOrNull("candidate_id");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(candidateId))
            {
                continue;
            }

            var officeFull = item.GetStringOrNull("office_full");
            var partyFull = item.GetStringOrNull("party_full");
            var state = item.GetStringOrNull("state");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = Name,
                Category = endpoint.Category,
                Title = $"{name} - {officeFull} candidate ({state})",
                Summary = partyFull is not null ? $"Party: {partyFull}" : null,
                Url = $"https://www.fec.gov/data/candidate/{candidateId}/",
                OriginalGuid = candidateId,
                Language = endpoint.Language,
                PublishedAt = item.GetDateTimeOrNull("load_date") ?? item.GetDateTimeOrNull("first_file_date"),
                Source = "FEC"
            });
        }

        return articles;
    }
}
