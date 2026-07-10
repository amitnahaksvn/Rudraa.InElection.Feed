using Mediator;
using Application.Abstractions;
using Domain.Enums;

namespace Application.News.Queries.GetNewsCountries;

/// <summary>Every distinct country represented in currently-active articles, optionally narrowed to one pipeline - backs the News Feed page's "view by country" filter.</summary>
public sealed record GetNewsCountriesQuery(ArticleSourceType? SourceType) : IRequest<IReadOnlyList<string>>;

public sealed class GetNewsCountriesQueryHandler : IRequestHandler<GetNewsCountriesQuery, IReadOnlyList<string>>
{
    private readonly INewsArticleRepository _articles;

    public GetNewsCountriesQueryHandler(INewsArticleRepository articles)
    {
        _articles = articles;
    }

    public async ValueTask<IReadOnlyList<string>> Handle(GetNewsCountriesQuery request, CancellationToken cancellationToken) =>
        await _articles.GetDistinctCountriesAsync(request.SourceType, cancellationToken);
}
