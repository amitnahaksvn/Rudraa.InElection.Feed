using Mediator;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Crawl.Dtos;
using Application.Models;
using Application.Options;
using Domain.Entities;
using Domain.Enums;

namespace Application.Crawl.Queries.GetCrawlReport;

/// <summary>Backs the crawl-report page's RSS/API tabs. <paramref name="From"/>/<paramref name="To"/> default to the trailing 7 days when omitted, so the page always has something to show the first time it's opened.</summary>
public sealed record GetCrawlReportQuery(CrawlPipeline Pipeline, DateTimeOffset? From, DateTimeOffset? To) : IRequest<CrawlReportDto>;

public sealed class GetCrawlReportQueryHandler : IRequestHandler<GetCrawlReportQuery, CrawlReportDto>
{
    // Generous ceiling on how many CrawlHistory docs one report aggregates in memory - comfortably
    // above any realistic window for this app's provider count/cron frequency, and a hard stop
    // against an accidentally huge date range turning this into an unbounded Mongo scan.
    private const int MaxRunsConsidered = 20_000;

    private readonly ICrawlHistoryRepository _history;
    private readonly ICrawlJobStatusReader _statusReader;
    private readonly NewsCrawlerOptions _rssOptions;
    private readonly NewsApiCrawlerOptions _apiOptions;

    public GetCrawlReportQueryHandler(
        ICrawlHistoryRepository history,
        ICrawlJobStatusReader statusReader,
        IOptions<NewsCrawlerOptions> rssOptions,
        IOptions<NewsApiCrawlerOptions> apiOptions)
    {
        _history = history;
        _statusReader = statusReader;
        _rssOptions = rssOptions.Value;
        _apiOptions = apiOptions.Value;
    }

    public async ValueTask<CrawlReportDto> Handle(GetCrawlReportQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddDays(-7);

        var runs = await _history.GetFilteredAsync(
            new CrawlHistoryFilter(request.Pipeline, Provider: null, from, to, Skip: 0, Take: MaxRunsConsidered),
            cancellationToken);

        var summary = BuildSummary(runs);
        var timeSeries = BuildTimeSeries(runs, from, to);
        var providers = BuildProviderBreakdown(request.Pipeline, runs);

        return new CrawlReportDto(request.Pipeline.ToString(), from, to, summary, timeSeries, providers);
    }

    private static CrawlReportSummaryDto BuildSummary(IReadOnlyList<CrawlHistory> runs)
    {
        var successful = runs.Count(r => r.Status == CrawlStatus.Completed);
        var withErrors = runs.Count(r => r.Status == CrawlStatus.CompletedWithErrors);
        var failed = runs.Count(r => r.Status == CrawlStatus.Failed);
        var skippedRuns = runs.Count(r => r.Status == CrawlStatus.Skipped);
        var newArticles = runs.Sum(r => r.NewArticles);
        var updatedArticles = runs.Sum(r => r.UpdatedArticles);
        var duplicateArticles = runs.Sum(r => r.DuplicateArticles);
        var failedFeeds = runs.Sum(r => r.FailedFeeds.Count);
        var totalRuns = runs.Count;
        var successRate = totalRuns == 0 ? 0 : Math.Round(successful * 100.0 / totalRuns, 1);

        return new CrawlReportSummaryDto(
            totalRuns,
            successful,
            withErrors,
            failed,
            skippedRuns,
            successRate,
            Messages: newArticles + updatedArticles + duplicateArticles,
            Saved: newArticles + updatedArticles,
            newArticles,
            updatedArticles,
            SkippedDuplicates: duplicateArticles,
            failedFeeds);
    }

    private static IReadOnlyList<CrawlReportDailyPointDto> BuildTimeSeries(
        IReadOnlyList<CrawlHistory> runs, DateTimeOffset from, DateTimeOffset to)
    {
        var byDay = runs.ToLookup(r => DateOnly.FromDateTime(r.StartTime.UtcDateTime));

        var points = new List<CrawlReportDailyPointDto>();
        for (var day = DateOnly.FromDateTime(from.UtcDateTime); day <= DateOnly.FromDateTime(to.UtcDateTime); day = day.AddDays(1))
        {
            var dayRuns = byDay[day].ToList();
            points.Add(new CrawlReportDailyPointDto(
                day,
                TotalRuns: dayRuns.Count,
                SuccessfulRuns: dayRuns.Count(r => r.Status == CrawlStatus.Completed),
                RunsWithErrors: dayRuns.Count(r => r.Status == CrawlStatus.CompletedWithErrors),
                FailedRuns: dayRuns.Count(r => r.Status == CrawlStatus.Failed),
                SkippedRuns: dayRuns.Count(r => r.Status == CrawlStatus.Skipped),
                NewArticles: dayRuns.Sum(r => r.NewArticles),
                UpdatedArticles: dayRuns.Sum(r => r.UpdatedArticles),
                DuplicateArticles: dayRuns.Sum(r => r.DuplicateArticles),
                FailedFeeds: dayRuns.Sum(r => r.FailedFeeds.Count)));
        }

        return points;
    }

    private IReadOnlyList<CrawlReportProviderDto> BuildProviderBreakdown(CrawlPipeline pipeline, IReadOnlyList<CrawlHistory> runs)
    {
        var configured = (pipeline == CrawlPipeline.Api
            ? _apiOptions.Countries.Where(c => c.Enabled).SelectMany(c => c.Providers.Where(p => p.Enabled).Select(p => (Country: c.Name, Provider: p.Name)))
            : _rssOptions.Countries.Where(c => c.Enabled).SelectMany(c => c.Providers.Where(p => p.Enabled).Select(p => (Country: c.Name, Provider: p.Name))))
            .ToList();

        // Fanned out across many providers concurrently, not sequentially - see
        // ICrawlJobStatusReader.GetStatuses's own doc comment for why that distinction matters at
        // this app's provider counts.
        var statuses = _statusReader.GetStatuses(pipeline, configured.Select(cp => cp.Provider).ToList());

        // Exact per-provider article/run attribution only applies to single-provider runs - the
        // normal case, since each provider's own scheduled Hangfire job crawls just that provider.
        // A manual "trigger everything" run (POST /api/crawl/trigger with no filter) can bundle
        // many providers into one CrawlHistory record; that activity still counts toward the
        // overall summary/time-series above, but is deliberately left out of any single
        // provider's row here rather than crediting the whole run's totals to every provider it
        // touched.
        var singleProviderRuns = runs
            .Where(r => r.Providers.Count == 1)
            .ToLookup(r => r.Providers[0], StringComparer.OrdinalIgnoreCase);

        var rows = new List<CrawlReportProviderDto>();
        foreach (var (country, providerName) in configured)
        {
            var providerRuns = singleProviderRuns[providerName].ToList();
            statuses.TryGetValue(providerName, out var status);

            // Failed-feed attribution stays exact even for multi-provider runs, since each entry
            // is its own "{Provider}/{Feed}" string regardless of how many providers ran together.
            var failedFeedPrefix = providerName + "/";
            var failedFeeds = runs.Sum(r => r.FailedFeeds.Count(f => f.StartsWith(failedFeedPrefix, StringComparison.OrdinalIgnoreCase)));

            var successful = providerRuns.Count(r => r.Status == CrawlStatus.Completed);
            var withErrors = providerRuns.Count(r => r.Status == CrawlStatus.CompletedWithErrors);
            var failed = providerRuns.Count(r => r.Status == CrawlStatus.Failed);
            var skipped = providerRuns.Count(r => r.Status == CrawlStatus.Skipped);
            var totalRuns = providerRuns.Count;

            rows.Add(new CrawlReportProviderDto(
                country,
                providerName,
                HasRun: totalRuns > 0,
                status?.Cron,
                status?.TimeZone,
                status?.NextExecution,
                status?.LastExecution,
                status?.LastJobState,
                status?.LastErrorMessage,
                totalRuns,
                successful,
                withErrors,
                failed,
                skipped,
                SuccessRatePercent: totalRuns == 0 ? 0 : Math.Round(successful * 100.0 / totalRuns, 1),
                NewArticles: providerRuns.Sum(r => r.NewArticles),
                UpdatedArticles: providerRuns.Sum(r => r.UpdatedArticles),
                DuplicateArticles: providerRuns.Sum(r => r.DuplicateArticles),
                failedFeeds));
        }

        return rows
            .OrderBy(r => r.Country, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Provider, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
