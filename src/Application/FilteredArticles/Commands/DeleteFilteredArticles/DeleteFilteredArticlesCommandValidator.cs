using FluentValidation;

namespace Application.FilteredArticles.Commands.DeleteFilteredArticles;

public sealed class DeleteFilteredArticlesCommandValidator : AbstractValidator<DeleteFilteredArticlesCommand>
{
    public DeleteFilteredArticlesCommandValidator()
    {
        RuleFor(c => c.Ids).NotEmpty();
        RuleForEach(c => c.Ids).NotEmpty();
    }
}
