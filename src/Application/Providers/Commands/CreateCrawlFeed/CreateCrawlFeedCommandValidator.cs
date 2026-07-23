using FluentValidation;
using Domain.Enums;

namespace Application.Providers.Commands.CreateCrawlFeed;

public sealed class CreateCrawlFeedCommandValidator : AbstractValidator<CreateCrawlFeedCommand>
{
    public CreateCrawlFeedCommandValidator()
    {
        RuleFor(c => c.Pipeline).Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api).WithMessage("Pipeline must be 'Rss' or 'Api'.");
        RuleFor(c => c.Provider).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Url).NotEmpty();
        RuleFor(c => c.Category).NotEmpty();
        RuleFor(c => c.Language).NotEmpty();
    }
}
