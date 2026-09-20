using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Abstractions;
using Application.Models;
using Application.Options;
using Domain.Enums;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Shared HTTP fetch + auth-injection pipeline for JSON news-API providers. Concrete providers
/// only supply <see cref="Name"/> and <see cref="ParseArticles"/> - everything about building each
/// request (base URL + endpoint path + configured query parameters + auth, entirely from
/// <see cref="NewsApiProviderOptions"/>/<see cref="NewsApiEndpointOptions"/>, never hardcoded per
/// provider) and error handling is common, mirroring how <c>BaseRssProvider</c> centralizes the
/// RSS fetch/parse pipeline across a provider's list of feeds.
/// </summary>
public abstract class BaseNewsApiProvider : INewsApiProvider
{
    /// <summary>Single shared named HttpClient for every news-API provider (see registration in InfrastructureServiceCollectionExtensions) - a 2-minute per-attempt timeout plus a 3-retry/5-10-20-minute-backoff Polly policy are both configured there, not per-call here.</summary>
    public const string HttpClientName = "NewsApiClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    protected BaseNewsApiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public abstract string Name { get; }

    /// <summary>Parses one endpoint's raw JSON response body into normalized articles - the only thing each concrete provider implements.</summary>
    protected abstract IReadOnlyList<NormalizedArticle> ParseArticles(string json, NewsApiEndpointOptions endpoint);

    /// <summary>Query parameter carrying the page number (2, 3, ...) for providers that page their results; null (default) = single request per endpoint.</summary>
    protected virtual string? PageParameterName => null;

    /// <summary>Size of a full page: the next page is only requested when the previous one returned at least this many articles.</summary>
    protected virtual int PageSize => int.MaxValue;

    /// <summary>Upper bound on pages fetched per endpoint per run (page 1 included).</summary>
    protected virtual int MaxPages => 1;

    /// <summary>How many times to re-send a request the API itself answered with HTTP 429 (0 = never, the default). For APIs whose limiter is intermittent rather than a hard daily quota.</summary>
    protected virtual int RateLimitRetries => 0;

    protected virtual TimeSpan RateLimitRetryDelay => TimeSpan.FromSeconds(8);

    private async Task<HttpResponseMessage> SendFirstPageAsync(
        HttpClient client, NewsApiProviderOptions options, NewsApiEndpointOptions endpoint, string? apiKey, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            using var request = BuildRequest(options, endpoint, apiKey);
            var response = await client.SendAsync(request, cancellationToken);

            // Only the API's own 429 is retried - our locally generated "daily budget spent" 429 carries
            // ApiQuotaHandler.ExceededHeader and must never be retried (that would be pointless).
            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests
                || response.Headers.Contains(ApiQuotaHandler.ExceededHeader)
                || attempt >= RateLimitRetries)
            {
                return response;
            }

            response.Dispose();
            _logger.LogInformation(
                "{Provider}/{Endpoint}: HTTP 429 from the API (attempt {Attempt}/{Max}) - retrying in {Delay}",
                options.Name, endpoint.Name, attempt + 1, RateLimitRetries + 1, RateLimitRetryDelay);
            await Task.Delay(RateLimitRetryDelay, cancellationToken);
        }
    }

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
        var url = BuildRequestUrl(options, endpoint, includeAuth: false);

        // Keyed by provider Name (not array index, which would silently break if Providers is
        // ever reordered) under "NewsApiKeys" in appsettings.json (development-tier credentials,
        // kept there by deliberate choice - see CLAUDE.md). AuthType.None (e.g. GDELT's public Doc
        // API) skips this lookup entirely - there's nothing to set.
        var apiKey = options.AuthType == ApiAuthType.None ? null : _configuration[$"NewsApiKeys:{options.Name}"];
        if (options.AuthType != ApiAuthType.None && string.IsNullOrWhiteSpace(apiKey))
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

        // No per-endpoint timeout wrapping this call (unlike most other fetches in this codebase) -
        // deliberately, so the HttpClient's own Polly retry policy (InfrastructureServiceCollectionExtensions,
        // 3 attempts with 5/10/20-minute gaps) gets the chance to actually run to completion instead
        // of being cut off after a short, fixed TimeoutSeconds. Each individual attempt is still
        // bounded by client.Timeout (2 minutes); cancellationToken (host shutdown) is the only thing
        // that can cut the whole retry sequence short.
        string? responseBody = null;
        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await SendFirstPageAsync(client, options, endpoint, apiKey, cancellationToken);
            httpStatusCode = (int)response.StatusCode;
            if (response.Headers.Contains(ApiQuotaHandler.ExceededHeader))
            {
                // Our own daily budget for this provider is spent - nothing was sent to the API, so
                // this is a skip, not a failure: no Error log, just an Information line.
                _logger.LogInformation(
                    "Skipping {Provider}/{Endpoint}: daily request limit ({Limit}) reached",
                    options.Name, endpoint.Name, options.DailyRequestLimit);
                return new ApiFetchResult
                {
                    EndpointName = endpoint.Name,
                    EndpointUrl = url,
                    Success = false,
                    QuotaExceeded = true,
                    Error = $"Daily request limit ({options.DailyRequestLimit}) reached for {options.Name}",
                    FetchedAt = fetchedAt,
                    HttpStatusCode = httpStatusCode,
                    ProcessingDurationMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Body read before the status check throws, not after, so a non-2xx response's body
            // (a JSON error payload, a rate-limit message) is still captured for
            // diagnostics/the monitoring-alert email instead of being discarded.
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            // Some APIs (APITube) echo the request URL - api key included - inside the JSON body, and
            // this body is later persisted/emailed/shown on the Test page, so redact it here, once.
            responseBody = RedactApiKey(responseBody, apiKey);
            response.EnsureSuccessStatusCode();

            var firstPage = ParseArticles(responseBody, endpoint);
            var allArticles = new List<NormalizedArticle>(firstPage);

            // Optional paging (off unless a provider overrides PageParameterName): a full page means
            // there may be more, so ask for the next page number - bounded by MaxPages and, since
            // every attempt goes through ApiQuotaHandler, by the provider's daily request limit.
            // Later pages are best-effort: a failure keeps what earlier pages already returned.
            var lastPageCount = firstPage.Count;
            for (var page = 2; PageParameterName is { } pageParam && page <= MaxPages && lastPageCount >= PageSize; page++)
            {
                var pageEndpoint = new NewsApiEndpointOptions
                {
                    Name = endpoint.Name,
                    Endpoint = endpoint.Endpoint,
                    QueryParameters = new Dictionary<string, string>(endpoint.QueryParameters) { [pageParam] = page.ToString() },
                    Category = endpoint.Category,
                    Language = endpoint.Language,
                    Enabled = endpoint.Enabled
                };

                using var pageRequest = BuildRequest(options, pageEndpoint, apiKey);
                using var pageResponse = await client.SendAsync(pageRequest, cancellationToken);
                if (!pageResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "{Provider}/{Endpoint}: page {Page} returned HTTP {StatusCode} - keeping the {Count} articles from earlier pages",
                        options.Name, endpoint.Name, page, (int)pageResponse.StatusCode, allArticles.Count);
                    break;
                }

                var pageBody = RedactApiKey(await pageResponse.Content.ReadAsStringAsync(cancellationToken), apiKey);
                var pageArticles = ParseArticles(pageBody, endpoint);
                allArticles.AddRange(pageArticles);
                lastPageCount = pageArticles.Count;
            }

            var json = responseBody;
            // Stamped here, once, rather than in every concrete provider's ParseArticles - every
            // JSON-API provider's articles are Api-sourced, so there's nothing provider-specific
            // about this assignment. NormalizedArticle is a record precisely so `with` works here.
            var articles = allArticles
                .Select(article => article with { SourceType = ArticleSourceType.Api })
                .ToList();

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
            // Same reasoning as BaseRssProvider/DynamicFeedIngestionService: a stray
            // OperationCanceledException from client.Timeout (already exhausted the Polly retry
            // policy's 3 attempts by this point) is a dead/rate-limited/slow API, not a real
            // shutdown request, and must be recorded as a failed run rather than crash the host.
            _logger.LogError(ex, "Failed to fetch/parse news API endpoint {Provider}/{Endpoint}", options.Name, endpoint.Name);
            return new ApiFetchResult
            {
                EndpointName = endpoint.Name,
                EndpointUrl = url,
                Success = false,
                Error = ex.Message,
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

    private static string RedactApiKey(string body, string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return body;
        }

        return body
            .Replace(apiKey, "***", StringComparison.Ordinal)
            .Replace(Uri.EscapeDataString(apiKey), "***", StringComparison.Ordinal);
    }

    private static HttpRequestMessage BuildRequest(NewsApiProviderOptions options, NewsApiEndpointOptions endpoint, string? apiKey)
    {
        var url = BuildRequestUrl(options, endpoint, includeAuth: options.AuthType == ApiAuthType.QueryParameter, apiKey);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (options.DailyRequestLimit is { } dailyLimit)
        {
            request.Options.Set(ApiQuotaHandler.ProviderKey, options.Name);
            request.Options.Set(ApiQuotaHandler.DailyLimitKey, dailyLimit);
        }

        if (options.AuthType == ApiAuthType.HttpHeader && apiKey is not null)
        {
            request.Headers.TryAddWithoutValidation(options.AuthParamName, apiKey);
        }

        return request;
    }

    private static string BuildRequestUrl(NewsApiProviderOptions options, NewsApiEndpointOptions endpoint, bool includeAuth, string? apiKey = null)
    {
        var queryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string root;

        // The endpoint may be stored as a full URL (so the Provider Management page shows exactly what
        // is called) instead of a path relative to BaseUrl. Any query string inside it is merged with
        // QueryParameters, which win on a clash - so the same parameter is never sent twice.
        if (Uri.TryCreate(endpoint.Endpoint, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https")
        {
            root = absolute.GetLeftPart(UriPartial.Path);
            var embedded = System.Web.HttpUtility.ParseQueryString(absolute.Query);
            foreach (var key in embedded.AllKeys.Where(k => !string.IsNullOrEmpty(k)))
            {
                queryParameters[key!] = embedded[key]!;
            }
        }
        else
        {
            root = $"{options.BaseUrl.TrimEnd('/')}/{endpoint.Endpoint.TrimStart('/')}";
        }

        foreach (var (key, value) in endpoint.QueryParameters)
        {
            queryParameters[key] = value;
        }

        if (includeAuth && apiKey is not null)
        {
            queryParameters[options.AuthParamName] = apiKey;
        }

        var query = string.Join('&', queryParameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        return query.Length == 0 ? root : $"{root}?{query}";
    }
}
