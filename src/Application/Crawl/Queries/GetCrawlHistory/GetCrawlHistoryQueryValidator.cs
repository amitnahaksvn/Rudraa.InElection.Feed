using FluentValidation;

namespace Application.Crawl.Queries.GetCrawlHistory;

public sealed class GetCrawlHistoryQueryValidator : AbstractValidator<GetCrawlHistoryQuery>
{
    public GetCrawlHistoryQueryValidator()
    {
        // Application-layer cap independent of any Web-layer clamping - the last line of defense
        // against an unbounded Mongo .Limit(count) for any caller of this query, not just today's
        // one HTTP endpoint (which itself does no clamping at all beyond defaulting count<=0 to 20).
        RuleFor(q => q.Count).GreaterThan(0).LessThanOrEqualTo(500);

        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || q.From <= q.To)
            .WithMessage("'From' must be less than or equal to 'To'.")
            .WithName("From");
    }
}
