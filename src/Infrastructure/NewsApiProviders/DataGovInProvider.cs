using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Models;
using Application.Options;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// data.gov.in's Open Government Data (OGD) platform (api.data.gov.in/resource/{resource-id}) -
/// query param <c>api-key</c>, plus <c>format=json</c>. Curl-verified the resource endpoint shape
/// is real, but unlike every other provider here, data.gov.in has no single generic "news"
/// dataset - every dataset is its own resource id with its own field schema, discoverable only by
/// browsing https://data.gov.in/catalogs and copying that dataset's resource id into
/// <see cref="NewsApiEndpointOptions.Endpoint"/>. This parser reads the common "records" wrapper
/// every OGD resource shares, plus a best-effort set of commonly-used field names (title/
/// description/date/link) - once a specific dataset is picked, its actual field names should be
/// confirmed against that dataset's own schema and this parser adjusted if they differ.
/// Wired but left disabled in configuration until a real, relevant resource id is chosen.
/// </summary>
public sealed class DataGovInProvider : BaseNewsApiProvider
{
    public const string ProviderName = "DataGovIn";

    public DataGovInProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<DataGovInProvider> logger)
        : base(httpClientFactory, configuration, logger)
    {
    }

    public override string Name => ProviderName;

    protected override IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("records", out var recordsElement))
        {
            return [];
        }

        var articles = new List<NormalizedArticle>();
        foreach (var item in recordsElement.EnumerateArray())
        {
            var title = item.GetStringOrNull("title") ?? item.GetStringOrNull("name") ?? item.GetStringOrNull("subject");
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var url = item.GetStringOrNull("link") ?? item.GetStringOrNull("url");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = endpoint.Name,
                Category = endpoint.Category,
                Title = title,
                Summary = item.GetStringOrNull("description"),
                Url = url ?? "https://data.gov.in",
                Language = endpoint.Language,
                PublishedAt = item.GetDateTimeOrNull("date"),
                Source = "data.gov.in"
            });
        }

        return articles;
    }
}
