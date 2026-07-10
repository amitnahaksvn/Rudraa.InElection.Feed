using Microsoft.Extensions.Options;
using Moq;
using Application.Abstractions;
using Application.Crawl.Queries.GetCrawlReport;
using Application.Models;
using Application.Options;
using Domain.Entities;
using Domain.Enums;

namespace PoliticalNews.Tests.Application;

public class GetCrawlReportQueryHandlerTests
{
    private static readonly DateTimeOffset Day1 = new(2026, 7, 1, 6, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Day2 = new(2026, 7, 2, 6, 0, 0, TimeSpan.Zero);

    private static CrawlHistory BuildRun(
        CrawlPipeline pipeline,
        DateTimeOffset startTime,
        IReadOnlyList<string> providers,
        CrawlStatus status,
        int newArticles = 0,
        int updatedArticles = 0,
        int duplicateArticles = 0,
        IReadOnlyList<string>? failedFeeds = null) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Pipeline = pipeline,
        Providers = providers.ToList(),
        StartTime = startTime,
        EndTime = startTime.AddMinutes(1),
        Duration = TimeSpan.FromMinutes(1),
        Status = status,
        NewArticles = newArticles,
        UpdatedArticles = updatedArticles,
        DuplicateArticles = duplicateArticles,
        FailedFeeds = (failedFeeds ?? []).ToList()
    };

    private static NewsCrawlerOptions BuildRssOptions() => new()
    {
        Countries =
        [
            new CountryOptions
            {
                Name = "India",
                Enabled = true,
                Providers =
                [
                    new RssProviderOptions { Name = "AajTak", Enabled = true },
                    new RssProviderOptions { Name = "ABPNews", Enabled = true }
                ]
            }
        ]
    };

    private GetCrawlReportQueryHandler BuildHandler(IReadOnlyList<CrawlHistory> runs, out Mock<ICrawlHistoryRepository> historyRepo)
    {
        historyRepo = new Mock<ICrawlHistoryRepository>();
        historyRepo
            .Setup(r => r.GetFilteredAsync(It.IsAny<CrawlHistoryFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        var statusReader = new Mock<ICrawlJobStatusReader>();
        statusReader
            .Setup(r => r.GetStatuses(It.IsAny<CrawlPipeline>(), It.IsAny<IReadOnlyCollection<string>>()))
            .Returns(new Dictionary<string, CrawlJobStatus>());

        return new GetCrawlReportQueryHandler(
            historyRepo.Object,
            statusReader.Object,
            Options.Create(BuildRssOptions()),
            Options.Create(new NewsApiCrawlerOptions()));
    }

    [Fact]
    public async Task Handle_AggregatesSummaryAcrossEveryMatchingRun()
    {
        var runs = new[]
        {
            BuildRun(CrawlPipeline.Rss, Day1, ["AajTak"], CrawlStatus.Completed, newArticles: 3, updatedArticles: 1, duplicateArticles: 2),
            BuildRun(CrawlPipeline.Rss, Day2, ["AajTak"], CrawlStatus.CompletedWithErrors, newArticles: 1, failedFeeds: ["AajTak/Home"]),
            BuildRun(CrawlPipeline.Rss, Day2, ["ABPNews"], CrawlStatus.Failed),
        };

        var handler = BuildHandler(runs, out _);
        var result = await handler.Handle(new GetCrawlReportQuery(CrawlPipeline.Rss, Day1, Day2), CancellationToken.None);

        Assert.Equal(3, result.Summary.TotalRuns);
        Assert.Equal(1, result.Summary.SuccessfulRuns);
        Assert.Equal(1, result.Summary.RunsWithErrors);
        Assert.Equal(1, result.Summary.FailedRuns);
        Assert.Equal(0, result.Summary.SkippedRuns);
        Assert.Equal(4, result.Summary.NewArticles); // 3 + 1
        Assert.Equal(1, result.Summary.UpdatedArticles);
        Assert.Equal(2, result.Summary.SkippedDuplicates);
        Assert.Equal(7, result.Summary.Messages); // 4 new + 1 updated + 2 duplicate
        Assert.Equal(5, result.Summary.Saved); // 4 new + 1 updated
        Assert.Equal(1, result.Summary.FailedFeeds);
        Assert.Equal(33.3, result.Summary.SuccessRatePercent); // 1 of 3 runs succeeded
    }

    [Fact]
    public async Task Handle_BucketsRunsByUtcCalendarDayForTheTimeSeries()
    {
        var runs = new[]
        {
            BuildRun(CrawlPipeline.Rss, Day1, ["AajTak"], CrawlStatus.Completed, newArticles: 5),
            BuildRun(CrawlPipeline.Rss, Day2, ["AajTak"], CrawlStatus.Completed, newArticles: 2),
        };

        var handler = BuildHandler(runs, out _);
        var result = await handler.Handle(new GetCrawlReportQuery(CrawlPipeline.Rss, Day1, Day2), CancellationToken.None);

        Assert.Equal(2, result.TimeSeries.Count);
        Assert.Equal(DateOnly.FromDateTime(Day1.UtcDateTime), result.TimeSeries[0].Date);
        Assert.Equal(5, result.TimeSeries[0].NewArticles);
        Assert.Equal(1, result.TimeSeries[0].TotalRuns);
        Assert.Equal(DateOnly.FromDateTime(Day2.UtcDateTime), result.TimeSeries[1].Date);
        Assert.Equal(2, result.TimeSeries[1].NewArticles);
    }

    [Fact]
    public async Task Handle_ProviderBreakdown_IncludesConfiguredProviderWithNoRunsAtAll()
    {
        var runs = new[] { BuildRun(CrawlPipeline.Rss, Day1, ["AajTak"], CrawlStatus.Completed, newArticles: 1) };

        var handler = BuildHandler(runs, out _);
        var result = await handler.Handle(new GetCrawlReportQuery(CrawlPipeline.Rss, Day1, Day2), CancellationToken.None);

        Assert.Equal(2, result.Providers.Count);
        var abpNews = Assert.Single(result.Providers, p => p.Provider == "ABPNews");
        Assert.False(abpNews.HasRun);
        Assert.Equal(0, abpNews.TotalRuns);
        Assert.Equal(0, abpNews.SuccessRatePercent);

        var aajTak = Assert.Single(result.Providers, p => p.Provider == "AajTak");
        Assert.True(aajTak.HasRun);
        Assert.Equal(1, aajTak.TotalRuns);
        Assert.Equal(100, aajTak.SuccessRatePercent);
    }

    [Fact]
    public async Task Handle_MultiProviderRun_ExcludedFromPerProviderArticleCountsButFailedFeedsStayAttributed()
    {
        // A manual "trigger everything" run bundles both providers into one record - exact
        // new/updated/duplicate attribution isn't possible per provider, so BuildProviderBreakdown
        // deliberately excludes it from either provider's own counters (it still lands in the
        // overall Summary, covered by the aggregation test above). Failed-feed strings are
        // "{Provider}/{Feed}" though, so that attribution stays exact regardless.
        var runs = new[]
        {
            BuildRun(CrawlPipeline.Rss, Day1, ["AajTak", "ABPNews"], CrawlStatus.CompletedWithErrors,
                newArticles: 10, failedFeeds: ["AajTak/Home", "ABPNews/Home"])
        };

        var handler = BuildHandler(runs, out _);
        var result = await handler.Handle(new GetCrawlReportQuery(CrawlPipeline.Rss, Day1, Day2), CancellationToken.None);

        var aajTak = Assert.Single(result.Providers, p => p.Provider == "AajTak");
        Assert.False(aajTak.HasRun);
        Assert.Equal(0, aajTak.NewArticles);
        Assert.Equal(1, aajTak.FailedFeeds);

        var abpNews = Assert.Single(result.Providers, p => p.Provider == "ABPNews");
        Assert.Equal(1, abpNews.FailedFeeds);
    }
}
