using FluentValidation;

namespace Application.News.Queries.SearchNews;

public sealed class SearchNewsQueryValidator : AbstractValidator<SearchNewsQuery>
{
    public SearchNewsQueryValidator()
    {
        RuleFor(q => q.Query).NotEmpty();
        // Application-layer cap, independent of Web's own ApiOptions.MaxPageSize clamping - the
        // last line of defense against an unbounded Mongo .Limit(count) for any caller of this query.
        RuleFor(q => q.Count).GreaterThan(0).LessThanOrEqualTo(500);
    }
}
