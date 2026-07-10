using Cronos;
using FluentValidation;
using Microsoft.Extensions.Options;
using Application.Options;
using Domain.Enums;

namespace Application.Providers.Commands.UpdateProviderSchedule;

public sealed class UpdateProviderScheduleCommandValidator : AbstractValidator<UpdateProviderScheduleCommand>
{
    public UpdateProviderScheduleCommandValidator(IOptions<NewsCrawlerOptions> rssOptions, IOptions<NewsApiCrawlerOptions> apiOptions)
    {
        // Country-level Enabled still gates which providers exist to schedule at all - only the
        // provider's own Enabled/Cron moved to the database. Deliberately NOT filtered by the
        // provider's own file Enabled, unlike CreateOrUpdateRecurringJobCommandValidator's older
        // RSS-only check - the whole point of this command is being able to enable a provider the
        // file currently has disabled.
        var rssProviders = rssOptions.Value.Countries
            .Where(c => c.Enabled)
            .SelectMany(c => c.Providers)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var apiProviders = apiOptions.Value.Countries
            .Where(c => c.Enabled)
            .SelectMany(c => c.Providers)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        RuleFor(c => c.Pipeline)
            .Must(p => p is CrawlPipeline.Rss or CrawlPipeline.Api)
            .WithMessage("Pipeline must be 'Rss' or 'Api'.");

        RuleFor(c => c)
            .Must(c => (c.Pipeline == CrawlPipeline.Api ? apiProviders : rssProviders).Contains(c.Provider))
            .WithMessage(c => $"'{c.Provider}' is not a configured {c.Pipeline} provider under an enabled country.")
            .WithName("Provider");

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
