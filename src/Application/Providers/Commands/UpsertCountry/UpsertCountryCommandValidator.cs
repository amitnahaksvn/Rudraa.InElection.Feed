using FluentValidation;
using Domain.Enums;

namespace Application.Providers.Commands.UpsertCountry;

public sealed class UpsertCountryCommandValidator : AbstractValidator<UpsertCountryCommand>
{
    public UpsertCountryCommandValidator()
    {
        RuleFor(c => c.Pipeline).Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api).WithMessage("Pipeline must be 'Rss' or 'Api'.");
        RuleFor(c => c.Name).NotEmpty();
    }
}
