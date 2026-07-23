using FluentValidation;
using Domain.Enums;

namespace Application.Providers.Queries.GetCountries;

public sealed class GetCountriesQueryValidator : AbstractValidator<GetCountriesQuery>
{
    public GetCountriesQueryValidator()
    {
        RuleFor(q => q.Pipeline).Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api).WithMessage("Pipeline must be 'Rss' or 'Api'.");
    }
}
