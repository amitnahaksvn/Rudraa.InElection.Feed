using Mediator;
using Application.Abstractions;

namespace Application.FilteredArticles.Commands.DeleteFilteredArticles;

/// <summary>
/// Deletes one or more <see cref="Domain.Entities.FilteredArticle"/> rows by id - backs both the
/// admin page's per-row delete button (a single id) and its multi-select bulk delete (many ids),
/// the same request shape either way, mirroring <c>DeleteArticlesCommand</c>. Unlike that command,
/// this is a hard delete (these rows are just a low-value diagnostic log, not a business record
/// worth soft-preserving). Returns how many were actually found and deleted.
/// </summary>
public sealed record DeleteFilteredArticlesCommand(IReadOnlyList<string> Ids) : IRequest<long>;

public sealed class DeleteFilteredArticlesCommandHandler : IRequestHandler<DeleteFilteredArticlesCommand, long>
{
    private readonly IFilteredArticleRepository _filteredArticles;

    public DeleteFilteredArticlesCommandHandler(IFilteredArticleRepository filteredArticles)
    {
        _filteredArticles = filteredArticles;
    }

    public async ValueTask<long> Handle(DeleteFilteredArticlesCommand request, CancellationToken cancellationToken) =>
        await _filteredArticles.DeleteManyAsync(request.Ids, cancellationToken);
}
