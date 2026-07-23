using FluentValidation;
using Application.Abstractions;
using Domain.Enums;

namespace Application.Crawl.Commands.TriggerProviderJob;

public sealed class TriggerProviderJobCommandValidator : AbstractValidator<TriggerProviderJobCommand>
{
    public TriggerProviderJobCommandValidator(ICrawlCountryRepository countries, IProviderScheduleRepository schedules)
    {
        RuleFor(c => c.Provider)
            .NotEmpty()
            .MustAsync(async (command, provider, cancellationToken) =>
            {
                var schedule = await schedules.GetAsync(command.Pipeline, provider, cancellationToken);
                if (schedule is null || !schedule.Enabled || string.IsNullOrWhiteSpace(schedule.Cron))
                {
                    return false;
                }

                var country = await countries.GetByNameAsync(command.Pipeline, schedule.Country, cancellationToken);
                return country is { Enabled: true };
            })
            .WithMessage(c => $"'{c.Provider}' is not an enabled {c.Pipeline} provider with a scheduled recurring job.");
    }
}
