using FluentValidation;
using Domain.Enums;

namespace Application.Crawl.Queries.GetArticleVolumeReport;

public sealed class GetArticleVolumeReportQueryValidator : AbstractValidator<GetArticleVolumeReportQuery>
{
    // Same cap as GetCrawlReportQueryValidator's own MaxRange - independent limits for an
    // independent query, kept numerically identical only because both reports share the same
    // "reasonable report window" reasoning.
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(365);

    public GetArticleVolumeReportQueryValidator()
    {
        // Same restriction as GetCrawlReportQueryValidator - ArticleFingerprint.SourceType only
        // ever records Rss or Api (Social-sourced articles are tagged Rss, since the underlying
        // fetch is still RSS/Atom under the hood), so there's no separate Social breakdown to ask for.
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
