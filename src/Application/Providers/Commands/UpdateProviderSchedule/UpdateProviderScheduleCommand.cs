using Mediator;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Options;
using Application.Providers.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Providers.Commands.UpdateProviderSchedule;

/// <summary>
/// Creates or overwrites one provider's schedule - the user-driven edit from the Provider
/// Management page's enable/disable toggle and cron editor. Persists to
/// <see cref="ProviderSchedule"/> (survives restarts, replacing
/// <c>NewsCrawler.appsettings.json</c>/<c>NewsApiCrawler</c>'s own Enabled/Cron as the source of
/// truth for this provider from now on) and immediately updates the live Hangfire recurring job -
/// enabling registers/reschedules it via <see cref="ICrawlJobTrigger.CreateOrUpdate"/>, disabling
/// removes it via <see cref="ICrawlJobTrigger.Remove"/>, so the effect is instant, not just "after
/// the next restart" the way the older <c>CreateOrUpdateRecurringJobCommand</c> live-override
/// still behaves for RSS.
/// </summary>
public sealed record UpdateProviderScheduleCommand(
    CrawlPipeline Pipeline,
    string Provider,
    bool Enabled,
    string Cron,
    string TimeZone = "UTC") : IRequest<ProviderScheduleDto>;

public sealed class UpdateProviderScheduleCommandHandler : IRequestHandler<UpdateProviderScheduleCommand, ProviderScheduleDto>
{
    private readonly IProviderScheduleRepository _schedules;
    private readonly ICrawlJobTrigger _jobTrigger;
    private readonly NewsCrawlerOptions _rssOptions;
    private readonly NewsApiCrawlerOptions _apiOptions;

    public UpdateProviderScheduleCommandHandler(
        IProviderScheduleRepository schedules,
        ICrawlJobTrigger jobTrigger,
        IOptions<NewsCrawlerOptions> rssOptions,
        IOptions<NewsApiCrawlerOptions> apiOptions)
    {
        _schedules = schedules;
        _jobTrigger = jobTrigger;
        _rssOptions = rssOptions.Value;
        _apiOptions = apiOptions.Value;
    }

    public async ValueTask<ProviderScheduleDto> Handle(UpdateProviderScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = new ProviderSchedule
        {
            Pipeline = request.Pipeline,
            Provider = request.Provider,
            Country = ResolveCountry(request.Pipeline, request.Provider),
            Enabled = request.Enabled,
            Cron = request.Cron,
            TimeZone = request.TimeZone,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _schedules.UpsertAsync(schedule, cancellationToken);

        if (request.Enabled)
        {
            _jobTrigger.CreateOrUpdate(request.Pipeline, request.Provider, request.Cron, request.TimeZone);
        }
        else
        {
            _jobTrigger.Remove(request.Pipeline, request.Provider);
        }

        return new ProviderScheduleDto(request.Pipeline.ToString(), request.Provider, request.Enabled, request.Cron, request.TimeZone);
    }

    private string ResolveCountry(CrawlPipeline pipeline, string provider) => pipeline == CrawlPipeline.Api
        ? _apiOptions.Countries.FirstOrDefault(c => c.Providers.Any(p => p.Name == provider))?.Name ?? string.Empty
        : _rssOptions.Countries.FirstOrDefault(c => c.Providers.Any(p => p.Name == provider))?.Name ?? string.Empty;
}
