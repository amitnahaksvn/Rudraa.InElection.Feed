using FluentValidation;

namespace Application.Providers.Commands.TestRssFeed;

public sealed class TestRssFeedCommandValidator : AbstractValidator<TestRssFeedCommand>
{
    public TestRssFeedCommandValidator()
    {
        RuleFor(c => c.FeedId).NotEmpty();
    }
}
