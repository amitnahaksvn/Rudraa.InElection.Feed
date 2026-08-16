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

    public ValueTask<IReadOnlyList<string>> Handle(GetNewsCountriesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"news-countries:{request.SourceType?.ToString() ?? "all"}";
        return new ValueTask<IReadOnlyList<string>>(_cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _articles.GetDistinctCountriesAsync(request.SourceType, cancellationToken);
        })!);
    }
}
