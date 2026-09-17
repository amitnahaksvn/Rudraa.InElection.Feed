using Mediator;
using Application.Abstractions;
using Application.Models;
using Application.News.Dtos;
using Domain.Enums;

namespace Application.News.Queries.GetNewsFeed;

/// <summary>
/// Backs the News Feed page's infinite scroll - ordered by <paramref name="SortBy"/>/
/// <paramref name="SortDirection"/> (PublishedAt/Descending, i.e. newest first, by default),
/// optionally narrowed to one pipeline (RSS/API tab) and/or one country. Pass
/// <paramref name="Cursor"/> (the PublishedAt/CrawledAt of the last article already shown, per
/// whichever <paramref name="SortBy"/> is in use) instead of relying on <paramref name="Skip"/> for
/// every page after the first - see <see cref="NewsArticleFeedFilter"/>'s own doc comment for why
/// Skip-based paging breaks once articles can be deleted mid-scroll. <paramref name="Skip"/> is
/// ignored whenever <paramref name="Cursor"/> is set.
/// </summary>
public sealed record GetNewsFeedQuery(
    ArticleSourceType? SourceType,
    string? Country,
    int Skip,
    int Count,
    NewsFeedSortBy SortBy = NewsFeedSortBy.PublishedAt,
    NewsFeedSortDirection SortDirection = NewsFeedSortDirection.Descending,
    DateTimeOffset? Cursor = null) : IRequest<IReadOnlyList<NewsArticleDto>>;

public sealed class GetNewsFeedQueryHandler : IRequestHandler<GetNewsFeedQuery, IReadOnlyList<NewsArticleDto>>
{
    private readonly INewsArticleRepository _articles;

    public GetNewsFeedQueryHandler(INewsArticleRepository articles)
    {
        _articles = articles;
    }

    public async ValueTask<IReadOnlyList<NewsArticleDto>> Handle(GetNewsFeedQuery request, CancellationToken cancellationToken)
    {
        var filter = new NewsArticleFeedFilter(request.SourceType, request.Country, request.Skip, request.Count, request.SortBy, request.SortDirection, request.Cursor);
        var articles = await _articles.GetFeedAsync(filter, cancellationToken);
        return articles.Select(NewsArticleDto.FromDomain).ToList();
    }
}
