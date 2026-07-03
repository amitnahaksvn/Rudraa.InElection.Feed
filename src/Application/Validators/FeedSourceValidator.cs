using FluentValidation;
using Domain.Entities;

namespace Application.Validators;

/// <summary>
/// Validates a <see cref="FeedSource"/> document before it's seeded or (in the future) before a
/// recurring job is registered from it - a <see cref="FeedSource"/> is inserted directly into
/// MongoDB rather than through a Mediator command, so this runs at those two call sites instead of
/// a pipeline behaviour.
/// </summary>
public sealed class FeedSourceValidator : AbstractValidator<FeedSource>
{
    public FeedSourceValidator()
    {
        RuleFor(f => f.SourceCode).NotEmpty();
        RuleFor(f => f.SourceName).NotEmpty();
        RuleFor(f => f.FeedName).NotEmpty();
        RuleFor(f => f.Category).NotEmpty();
        RuleFor(f => f.Language).NotEmpty();
        RuleFor(f => f.Country).NotEmpty();

        RuleFor(f => f.FeedUrl)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("FeedUrl must be an absolute http(s) URL.");

        RuleFor(f => f.FetchIntervalMinutes).GreaterThan(0);
        RuleFor(f => f.TimeoutSeconds).GreaterThan(0);
    }
}
