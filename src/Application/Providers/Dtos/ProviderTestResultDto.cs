using Application.Models;

namespace Application.Providers.Dtos;

/// <summary>Outcome of an on-demand "Test" click from the Provider Management page - the same shape regardless of whether it came from an RSS feed (<see cref="FeedFetchResult"/>) or a JSON-API endpoint (<see cref="ApiFetchResult"/>), since both already carry an equivalent set of diagnostics. <see cref="RawResponseBody"/> is the feed's raw XML or the endpoint's raw JSON, verbatim - lets the page show exactly what the provider actually returned rather than just the parsed article count.</summary>
public sealed record ProviderTestResultDto(
    bool Success,
    int? HttpStatusCode,
    int ArticleCount,
    long ProcessingDurationMs,
    DateTimeOffset FetchedAt,
    string? Error,
    string? ExceptionType,
    string? RawResponseBody)
{
    public static ProviderTestResultDto FromFeedResult(FeedFetchResult result) => new(
        result.Success,
        result.HttpStatusCode,
        result.Articles.Count,
        result.ProcessingDurationMs,
        result.FetchedAt,
        result.Error,
        result.ExceptionType,
        result.RawXml);

    public static ProviderTestResultDto FromApiResult(ApiFetchResult result) => new(
        result.Success,
        result.HttpStatusCode,
        result.Articles.Count,
        result.ProcessingDurationMs,
        result.FetchedAt,
        result.Error,
        result.ExceptionType,
        result.ResponseBody);

    /// <summary>A test that never reached a provider at all - e.g. the requested provider/feed/endpoint name doesn't exist in current configuration (most likely it was disabled or renamed since the page was loaded).</summary>
    public static ProviderTestResultDto NotFound(string error) => new(false, null, 0, 0, DateTimeOffset.UtcNow, error, null, null);
}
