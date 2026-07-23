using Cronos;
using FluentValidation;
using Application.Abstractions;
using Domain.Enums;

namespace Application.Crawl.Commands.CreateOrUpdateRecurringJob;

public sealed class CreateOrUpdateRecurringJobCommandValidator : AbstractValidator<CreateOrUpdateRecurringJobCommand>
{
    public CreateOrUpdateRecurringJobCommandValidator(ICrawlCountryRepository countries, IProviderScheduleRepository schedules)
    {
        RuleFor(c => c.JobName)
            .NotEmpty()
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var schedule = await schedules.GetAsync(command.Pipeline, name, cancellationToken);
                if (schedule is null || !schedule.Enabled)
                {
                    return false;
                }

                var country = await countries.GetByNameAsync(command.Pipeline, schedule.Country, cancellationToken);
                return country is { Enabled: true };
            })
            .WithMessage(c => $"'{c.JobName}' is not an enabled {c.Pipeline} provider - configure it first.");

        RuleFor(c => c.Cron)
            .NotEmpty()
            .Must(BeAValidCronExpression)
            .WithMessage("Cron must be a valid standard 5-field cron expression, e.g. '*/5 * * * *'.");

        RuleFor(c => c.TimeZone)
            .NotEmpty()
            .Must(BeAValidTimeZone)
            .WithMessage(c => $"'{c.TimeZone}' is not a recognized time zone id (e.g. 'UTC', 'Asia/Kolkata').");
    }

    private static bool BeAValidCronExpression(string cron)
    {
        try
        {
            CronExpression.Parse(cron, CronFormat.Standard);
            return true;
        }
        catch (CronFormatException)
        {
            return false;
        }
    }

    private static bool BeAValidTimeZone(string timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return false;
        }
    }
}
