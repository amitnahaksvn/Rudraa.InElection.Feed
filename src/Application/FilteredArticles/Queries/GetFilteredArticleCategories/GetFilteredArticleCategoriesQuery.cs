using Mediator;
using Application.Abstractions;

namespace Application.FilteredArticles.Queries.GetFilteredArticleCategories;

/// <summary>Every distinct category currently represented in FilteredArticles - backs the admin page's category filter.</summary>
public sealed record GetFilteredArticleCategoriesQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetFilteredArticleCategoriesQueryHandler : IRequestHandler<GetFilteredArticleCategoriesQuery, IReadOnlyList<string>>
{
    private readonly IFilteredArticleRepository _filteredArticles;

    public GetFilteredArticleCategoriesQueryHandler(IFilteredArticleRepository filteredArticles)
    {
        _filteredArticles = filteredArticles;
    }

    public async ValueTask<IReadOnlyList<string>> Handle(GetFilteredArticleCategoriesQuery request, CancellationToken cancellationToken) =>
        await _filteredArticles.GetDistinctCategoriesAsync(cancellationToken);
}
