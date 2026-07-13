using Mediator;
using Application.Abstractions;

namespace Application.News.Commands.DeleteArticles;

/// <summary>
/// Deletes one or more articles by id - backs both the News Feed page's per-card delete button
/// (a single id) and its multi-select bulk delete (many ids), the same request shape either way.
/// See <see cref="INewsArticleRepository.DeleteManyAsync"/> for why this is a soft delete
/// (IsActive=false), not a document removal.
/// </summary>
public sealed record DeleteArticlesCommand(IReadOnlyList<string> Ids) : IRequest<long>;

public sealed class DeleteArticlesCommandHandler : IRequestHandler<DeleteArticlesCommand, long>
{
    private readonly INewsArticleRepository _repository;

    public DeleteArticlesCommandHandler(INewsArticleRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<long> Handle(DeleteArticlesCommand request, CancellationToken cancellationToken) =>
        await _repository.DeleteManyAsync(request.Ids, cancellationToken);
}
