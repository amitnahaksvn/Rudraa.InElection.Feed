using FluentValidation;

namespace Application.News.Commands.DeleteArticles;

public sealed class DeleteArticlesCommandValidator : AbstractValidator<DeleteArticlesCommand>
{
    public DeleteArticlesCommandValidator()
    {
        RuleFor(c => c.Ids).NotEmpty();
        RuleForEach(c => c.Ids).NotEmpty();
    }
}
