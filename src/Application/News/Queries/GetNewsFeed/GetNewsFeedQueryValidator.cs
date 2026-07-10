using FluentValidation;

namespace Application.News.Queries.GetNewsFeed;

public sealed class GetNewsFeedQueryValidator : AbstractValidator<GetNewsFeedQuery>
{
    public GetNewsFeedQueryValidator()
    {
        RuleFor(q => q.Count).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.Skip).GreaterThanOrEqualTo(0);
    }
}
