using FluentValidation;

namespace PoliticalNews.Application.Crawl.Queries.GetCrawlJobStatus;

public sealed class GetCrawlJobStatusQueryValidator : AbstractValidator<GetCrawlJobStatusQuery>
{
    public GetCrawlJobStatusQueryValidator()
    {
        RuleFor(q => q.Provider).NotEmpty();
    }
}
