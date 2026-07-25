using Mediator;
using Application.Abstractions;
using Application.Crawl.Dtos;
using Domain.Enums;

namespace Application.Crawl.Queries.GetArticleVolumeReport;

/// <summary>
/// Backs the article-volume report page - the same shape of question as
/// <see cref="Application.Crawl.Queries.GetCrawlReport.GetCrawlReportQuery"/> (pipeline tab, date
/// range, optional multi-provider filter) but sourced entirely from
/// <see cref="IArticleFingerprintRepository"/> rather than CrawlHistory/Hangfire: "how many
/// articles actually came in per provider", not schedule/success-rate. <paramref name="From"/>/
/// <paramref name="To"/> default to the trailing 7 days when omitted. <paramref name="Providers"/>
/// mirrors GetCrawlReportQuery's own filter semantics exactly - null/empty lists every enabled
/// provider under an enabled country; a non-empty selection lists exactly those providers
/// regardless of their own or their country's Enabled flag, the only way to surface a disabled
/// provider's past volume.
/// </summary>
public sealed record GetArticleVolumeReportQuery(
    CrawlPipeline Pipeline, DateTimeOffset? From, DateTimeOffset? To, IReadOnlyList<string>? Providers = null) : IRequest<ArticleVolumeReportDto>;

public sealed class GetArticleVolumeReportQueryHandler : IRequestHandler<GetArticleVolumeReportQuery, ArticleVolumeReportDto>
{
    private readonly IArticleFingerprintRepository _fingerprints;
    private readonly ICrawlCountryRepository _countryRepository;
    private readonly IProviderScheduleRepository _scheduleRepository;

    public GetArticleVolumeReportQueryHandler(
        IArticleFingerprintRepository fingerprints,
        ICrawlCountryRepository countryRepository,
        IProviderScheduleRepository scheduleRepository)
    {
        _fingerprints = fingerprints;
        _countryRepository = countryRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async ValueTask<ArticleVolumeReportDto> Handle(GetArticleVolumeReportQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddDays(-7);

        var selectedProviders = request.Providers is { Count: > 0 }
            ? new HashSet<string>(request.Providers, StringComparer.OrdinalIgnoreCase)
            : null;

        var sourceType = request.Pipeline == CrawlPipeline.Api ? ArticleSourceType.Api : ArticleSourceType.Rss;

        var counts = await _fingerprints.GetDailyProviderCountsAsync(sourceType, from, to, cancellationToken);
        if (selectedProviders is not null)
        {
            counts = counts.Where(c => selectedProviders.Contains(c.Provider)).ToList();
        }

        var countsByProvider = counts
            .GroupBy(c => c.Provider, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Count), StringComparer.OrdinalIgnoreCase);

        // Same "configured provider list" source of truth GetCrawlReportQuery uses - enabled
        // providers under an enabled country by default, or exactly the caller's explicit
        // selection (including disabled ones) when given. Deduplicated to bare provider name since
        // ArticleFingerprint carries no Country dimension - a provider scheduled under more than
        // one country contributes one combined row here, not one row per country.
        var countries = await _countryRepository.GetAllAsync(request.Pipeline, cancellationToken);
        var enabledCountryNames = new HashSet<string>(
            countries.Where(c => c.Enabled).Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        var schedules = await _scheduleRepository.GetAllAsync(request.Pipeline, cancellationToken);
        var configuredProviders = schedules
            .Where(s => selectedProviders is not null
                ? selectedProviders.Contains(s.Provider)
                : s.Enabled && enabledCountryNames.Contains(s.Country))
            .Select(s => s.Provider)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var providerRows = configuredProviders
            .Select(p => new ArticleVolumeProviderDto(p, countsByProvider.GetValueOrDefault(p)))
            .OrderByDescending(r => r.Count)
            .ThenBy(r => r.Provider, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Sparse (day, provider) rows straight from GetDailyProviderCountsAsync's own grouping -
        // no need to zero-fill every (day, provider) combination server-side; the frontend already
        // knows the full date range being charted and fills in zeros for whichever days/providers
        // it renders.
        var providerTimeSeries = counts
            .Select(c => new ArticleVolumeProviderDailyPointDto(c.Date, c.Provider, c.Count))
            .ToList();

        return new ArticleVolumeReportDto(
            request.Pipeline.ToString(),
            from,
            to,
            counts.Sum(c => c.Count),
            providerTimeSeries,
            providerRows);
    }
}
