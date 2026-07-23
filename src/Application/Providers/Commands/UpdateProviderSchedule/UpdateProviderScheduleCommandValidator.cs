using Cronos;
using FluentValidation;
using Application.Abstractions;
using Domain.Enums;

namespace Application.Providers.Commands.UpdateProviderSchedule;

public sealed class UpdateProviderScheduleCommandValidator : AbstractValidator<UpdateProviderScheduleCommand>
{
    public UpdateProviderScheduleCommandValidator(
        ICrawlCountryRepository countries,
        IEnumerable<IRssProvider> rssProviders,
        IEnumerable<INewsApiProvider> apiProviders)
    {
        var rssProviderNames = rssProviders.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var apiProviderNames = apiProviders.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        RuleFor(c => c.Pipeline)
            .Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api)
            .WithMessage("Pipeline must be 'Rss' or 'Api'.");

        RuleFor(c => c.Provider).NotEmpty();

        // A provider name only actually does anything once a matching IRssProvider/INewsApiProvider
        // C# class is registered - adding a row here for a name with no such class would silently
        // never crawl (just a log warning at the next tick), so this is caught up front instead.
        RuleFor(c => c)
            .Must(c => (c.Pipeline == CrawlPipeline.Api ? apiProviderNames : rssProviderNames).Contains(c.Provider))
            .WithMessage(c => $"'{c.Provider}' has no registered I{c.Pipeline}Provider implementation - add that C# class first.")
            .WithName("Provider");

        RuleFor(c => c.Country).NotEmpty();

        RuleFor(c => c)
            .MustAsync(async (c, cancellationToken) => await countries.GetByNameAsync(c.Pipeline, c.Country, cancellationToken) is not null)
            .WithMessage(c => $"'{c.Country}' is not a configured {c.Pipeline} country - add it first.")
            .WithName("Country");

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
