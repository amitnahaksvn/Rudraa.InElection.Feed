using FluentValidation;

namespace Application.Providers.Commands.UpdateCrawlFeed;

public sealed class UpdateCrawlFeedCommandValidator : AbstractValidator<UpdateCrawlFeedCommand>
{
    public UpdateCrawlFeedCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Url).NotEmpty();
        RuleFor(c => c.Category).NotEmpty();
        RuleFor(c => c.Language).NotEmpty();
    }
}
