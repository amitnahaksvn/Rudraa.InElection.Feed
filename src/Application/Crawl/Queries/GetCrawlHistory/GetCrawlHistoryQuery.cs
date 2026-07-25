using Mediator;
using Application.Abstractions;
using Application.Crawl.Dtos;
using Application.Models;
using Domain.Enums;

namespace Application.Crawl.Queries.GetCrawlHistory;

/// <summary>Every filter beyond <paramref name="Count"/> is optional - omitting all of them reproduces the old "most recent N runs of anything" behaviour; the crawl-report page narrows by <paramref name="Pipeline"/>/<paramref name="Provider"/>/date range to browse one pipeline or provider's own history at any point in time. <paramref name="Providers"/> is the crawl-report page's multi-provider filter selection - additive to <paramref name="Provider"/>, matching a run if ANY of its own providers is in the given set.</summary>
public sealed record GetCrawlHistoryQuery(
    int Count,
    CrawlPipeline? Pipeline = null,
    string? Provider = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Skip = 0,
    IReadOnlyList<string>? Providers = null) : IRequest<IReadOnlyList<CrawlHistoryDto>>;

public sealed class GetCrawlHistoryQueryHandler : IRequestHandler<GetCrawlHistoryQuery, IReadOnlyList<CrawlHistoryDto>>
{
    private readonly ICrawlHistoryRepository _history;

    public GetCrawlHistoryQueryHandler(ICrawlHistoryRepository history)
    {
        _history = history;
    }

    public async ValueTask<IReadOnlyList<CrawlHistoryDto>> Handle(GetCrawlHistoryQuery request, CancellationToken cancellationToken)
    {
        var filter = new CrawlHistoryFilter(request.Pipeline, request.Provider, request.From, request.To, request.Skip, request.Count, request.Providers);
        var history = await _history.GetFilteredAsync(filter, cancellationToken);
        return history.Select(CrawlHistoryDto.FromDomain).ToList();
    }
}
