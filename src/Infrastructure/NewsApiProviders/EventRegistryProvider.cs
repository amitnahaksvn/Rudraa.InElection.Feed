using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Abstractions;
using Application.Models;
using Application.Options;
using Domain.Enums;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// NewsAPI.ai / Event Registry (eventregistry.org/api/v1/article/getArticles) - the one provider
/// in this pipeline that needs a JSON POST body rather than a GET with a query-param/header key,
/// so it implements <see cref="INewsApiProvider"/> directly instead of extending
/// <see cref="BaseNewsApiProvider"/> - same reasoning <c>YouTubeRssProvider</c> implements
/// <c>IRssProvider</c> directly for Atom instead of extending <c>BaseRssProvider</c>: a
/// fundamentally different request shape isn't worth forcing into the shared GET-based pipeline.
/// Each configured endpoint's <see cref="NewsApiEndpointOptions.QueryParameters"/> become POST
/// body fields verbatim (merged over a few fixed defaults below) - e.g. an entry
/// <c>{"keyword": "India politics", "lang": "eng"}</c> controls the actual Event Registry query,
/// fully configurable with no code change. Auth is the <c>apiKey</c> field inside the POST body
/// itself, still sourced from <c>NewsApiKeys:EventRegistry</c> like every other provider's key.
/// Free plan: limited daily token allowance. https://newsapi.ai/plans
/// </summary>
public sealed class EventRegistryProvider : INewsApiProvider
{
    public const string ProviderName = "EventRegistry";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventRegistryProvider> _logger;

    public EventRegistryProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EventRegistryProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public string Name => ProviderName;

    public async Task<IReadOnlyList<ApiFetchResult>> FetchAllEndpointsAsync(NewsApiProviderOptions options, CancellationToken cancellationToken)
    {
        var enabledEndpoints = options.Endpoints.Where(e => e.Enabled).ToList();
        var results = new List<ApiFetchResult>(enabledEndpoints.Count);

        foreach (var endpoint in enabledEndpoints)
        {
            results.Add(await FetchEndpointAsync(options, endpoint, cancellationToken));
        }

        return results;
    }

    private async Task<ApiFetchResult> FetchEndpointAsync(NewsApiProviderOptions options, NewsApiEndpointOptions endpoint, CancellationToken cancellationToken)
    {
        var fetchedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        int? httpStatusCode = null;
        string? responseBody = null;
        var url = options.BaseUrl;

        var apiKey = _configuration[$"NewsApiKeys:{options.Name}"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning(
                "No API key configured for news API provider '{Provider}' (NewsApiKeys:{Provider}) - skipping endpoint {Endpoint}",
                options.Name, options.Name, endpoint.Name);
            return new ApiFetchResult
            {
                EndpointName = endpoint.Name,
                EndpointUrl = url,
                Success = false,
                Error = $"No API key configured under NewsApiKeys:{options.Name}",
                FetchedAt = fetchedAt,
                ProcessingDurationMs = stopwatch.ElapsedMilliseconds
            };
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

        try
        {
            var body = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["action"] = "getArticles",
                ["resultType"] = "articles",
                ["dataType"] = new[] { "news" },
                ["articlesSortBy"] = "date",
                ["articlesCount"] = 50,
                ["apiKey"] = apiKey
            };
            foreach (var (key, value) in endpoint.QueryParameters)
            {
                body[key] = value;
            }

            var client = _httpClientFactory.CreateClient(BaseNewsApiProvider.HttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };

            using var response = await client.SendAsync(request, timeoutCts.Token);
            httpStatusCode = (int)response.StatusCode;
            // Body read before the status check throws, not after, so a non-2xx response's body
            // (a JSON error payload, a rate-limit message) is still captured for
            // diagnostics/the monitoring-alert email instead of being discarded - same reasoning
            // as BaseNewsApiProvider.FetchEndpointAsync.
            responseBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            var articles = ParseArticles(responseBody, endpoint);

            return new ApiFetchResult
            {
                EndpointName = endpoint.Name,
                EndpointUrl = url,
                Success = true,
                Articles = articles,
                ResponseBody = responseBody,
                FetchedAt = fetchedAt,
                HttpStatusCode = httpStatusCode,
                ProcessingDurationMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Failed to fetch/parse news API endpoint {Provider}/{Endpoint}", options.Name, endpoint.Name);
            return new ApiFetchResult
            {
                EndpointName = endpoint.Name,
                EndpointUrl = url,
                Success = false,
                Error = ex.Message,
                // Same diagnostic fields BaseNewsApiProvider's own catch block populates for every
                // other provider - previously missing here, so Event Registry failures showed
                // blank exception detail in monitoring emails where every other provider doesn't.
                ExceptionType = ex.GetType().FullName ?? ex.GetType().Name,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException is { } inner ? $"{inner.GetType().FullName}: {inner.Message}" : null,
                ResponseBody = responseBody,
                FetchedAt = fetchedAt,
                HttpStatusCode = httpStatusCode,
                ProcessingDurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("articles", out var articlesWrapper) ||
            !articlesWrapper.TryGetProperty("results", out var resultsElement))
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

            var sourceName = item.TryGetProperty("source", out var source) ? source.GetStringOrNull("title") : null;
            var author = item.TryGetProperty("authors", out var authors) && authors.ValueKind == JsonValueKind.Array
                ? authors.EnumerateArray().Select(a => a.GetStringOrNull("name")).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                : null;
            var body = item.GetStringOrNull("body");

            articles.Add(new NormalizedArticle
            {
                Provider = Name,
                FeedName = sourceName ?? Name,
                Category = endpoint.Category,
                Title = title,
                Summary = body is { Length: > 300 } ? string.Concat(body.AsSpan(0, 300), "...") : body,
                Content = body,
                Url = url,
                OriginalGuid = item.GetStringOrNull("uri"),
                Author = author,
                Language = item.GetStringOrNull("lang") ?? endpoint.Language,
                ImageUrl = item.GetStringOrNull("image"),
                PublishedAt = item.GetDateTimeOrNull("dateTimePub") ?? item.GetDateTimeOrNull("dateTime"),
                Source = sourceName ?? "NewsAPI.ai (Event Registry)",
                SourceType = ArticleSourceType.Api
            });
        }

        return articles;
    }
}
