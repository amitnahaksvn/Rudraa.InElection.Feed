using Mediator;
using Application.Abstractions;

namespace Application.FilteredArticles.Queries.GetFilteredArticleProviders;

/// <summary>Every distinct provider currently represented in FilteredArticles - backs the admin page's provider filter.</summary>
public sealed record GetFilteredArticleProvidersQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetFilteredArticleProvidersQueryHandler : IRequestHandler<GetFilteredArticleProvidersQuery, IReadOnlyList<string>>
{
    private readonly IFilteredArticleRepository _filteredArticles;

    public GetFilteredArticleProvidersQueryHandler(IFilteredArticleRepository filteredArticles)
    {
        _filteredArticles = filteredArticles;
    }

    public async ValueTask<IReadOnlyList<string>> Handle(GetFilteredArticleProvidersQuery request, CancellationToken cancellationToken) =>
        await _filteredArticles.GetDistinctProvidersAsync(cancellationToken);
}
