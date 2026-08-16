using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Application.Abstractions;
using Domain.Enums;

namespace Application.News.Queries.GetNewsCountries;

/// <summary>Every distinct country represented in currently-active articles, optionally narrowed to one pipeline - backs the News Feed page's "view by country" filter.</summary>
public sealed record GetNewsCountriesQuery(ArticleSourceType? SourceType) : IRequest<IReadOnlyList<string>>;

/// <summary>
/// Cached for 5 minutes, keyed by <see cref="GetNewsCountriesQuery.SourceType"/> - the underlying
/// query is a Mongo <c>distinct</c> over every active article (60k+ in production), which is one
/// of the most RU-expensive reads this app makes against Cosmos DB and is fired on every single
/// News Feed page load. The set of distinct countries changes on the order of "a new country's
/// first article ever crawled," not second to second, so a few minutes of staleness is invisible
/// in practice - confirmed live: this exact query was hitting Cosmos DB 429 throttling under
/// ordinary concurrent page-load traffic even after raising NewsArticles' provisioned RU/s, and
/// re-querying it on every request was the actual root cause, not insufficient throughput alone.
///
/// Wrapped in <see cref="Lazy{T}"/> (not a bare <c>GetOrCreateAsync</c> factory) specifically for
/// single-flight/stampede protection: plain <c>IMemoryCache.GetOrCreateAsync</c> does not lock per
/// key, so N concurrent requests against a cold cache (confirmed live right after a deploy, or
/// whenever the 5-minute entry expires under real concurrent traffic) each independently re-run
/// the expensive query instead of one populating it for the rest - <see cref="Lazy{T}"/>'s default
/// thread-safety mode guarantees the factory delegate runs exactly once even when multiple callers
/// request it simultaneously. On failure the cache entry is evicted immediately (rather than
/// caching a faulted <see cref="Lazy{T}"/> for the rest of the 5-minute window) so a transient
/// Cosmos throttle doesn't poison every request until expiry.
/// </summary>
public sealed class GetNewsCountriesQueryHandler : IRequestHandler<GetNewsCountriesQuery, IReadOnlyList<string>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly INewsArticleRepository _articles;
    private readonly IMemoryCache _cache;

    public GetNewsCountriesQueryHandler(INewsArticleRepository articles, IMemoryCache cache)
    {
        _articles = articles;
        _cache = cache;
    }

    public async ValueTask<IReadOnlyList<string>> Handle(GetNewsCountriesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"news-countries:{request.SourceType?.ToString() ?? "all"}";

        var lazyResult = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return new Lazy<Task<IReadOnlyList<string>>>(
                () => _articles.GetDistinctCountriesAsync(request.SourceType, cancellationToken),
                LazyThreadSafetyMode.ExecutionAndPublication);
        })!;

        try
        {
            return await lazyResult.Value;
        }
        catch
        {
            _cache.Remove(cacheKey);
            throw;
        }
    }
}
