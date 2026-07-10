using FluentValidation;
using Domain.Enums;

namespace Application.Crawl.Queries.GetCrawlReport;

public sealed class GetCrawlReportQueryValidator : AbstractValidator<GetCrawlReportQuery>
{
    // Max lookback window - a hard cap independent of GetCrawlReportQueryHandler's own
    // MaxRunsConsidered record cap, so an over-wide range 400s with a clear message instead of
    // silently truncating results.
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(365);

    public GetCrawlReportQueryValidator()
    {
        // Only RSS and API have a crawl-report tab (Social has no schedule/success-rate reporting
        // built for it yet) - reject rather than silently mislabel Social data as one of the two.
        RuleFor(q => q.Pipeline).Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api)
            .WithMessage("Pipeline must be 'Rss' or 'Api'.");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || q.From <= q.To)
            .WithMessage("'From' must be less than or equal to 'To'.")
            .WithName("From");

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || q.To.Value - q.From.Value <= MaxRange)
            .WithMessage($"The date range cannot exceed {MaxRange.Days} days.")
            .WithName("From");
    }
}
