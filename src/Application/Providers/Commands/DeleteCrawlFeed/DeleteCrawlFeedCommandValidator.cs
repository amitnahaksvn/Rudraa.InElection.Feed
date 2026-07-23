using FluentValidation;

namespace Application.Providers.Commands.DeleteCrawlFeed;

public sealed class DeleteCrawlFeedCommandValidator : AbstractValidator<DeleteCrawlFeedCommand>
{
    public DeleteCrawlFeedCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
