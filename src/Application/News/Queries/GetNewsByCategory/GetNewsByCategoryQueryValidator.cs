using FluentValidation;

namespace Application.News.Queries.GetNewsByCategory;

public sealed class GetNewsByCategoryQueryValidator : AbstractValidator<GetNewsByCategoryQuery>
{
    public GetNewsByCategoryQueryValidator()
    {
        RuleFor(q => q.Category).NotEmpty();
        // Application-layer cap, independent of Web's own ApiOptions.MaxPageSize clamping - the
        // last line of defense against an unbounded Mongo .Limit(count) for any caller of this query.
        RuleFor(q => q.Count).GreaterThan(0).LessThanOrEqualTo(500);
    }
}
