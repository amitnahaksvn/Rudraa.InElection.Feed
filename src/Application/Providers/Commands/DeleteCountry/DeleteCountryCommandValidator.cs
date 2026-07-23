using FluentValidation;
using Domain.Enums;

namespace Application.Providers.Commands.DeleteCountry;

public sealed class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
{
    public DeleteCountryCommandValidator()
    {
        RuleFor(c => c.Pipeline).Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api).WithMessage("Pipeline must be 'Rss' or 'Api'.");
        RuleFor(c => c.Name).NotEmpty();
    }
}
