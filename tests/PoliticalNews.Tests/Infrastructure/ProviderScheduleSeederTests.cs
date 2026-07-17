using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Application.Abstractions;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Seed;

namespace PoliticalNews.Tests.Infrastructure;

public class ProviderScheduleSeederTests
{
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
                    new RssProviderOptions
                    {
                        Name = "AajTak",
                        Enabled = true,
                        Cron = "3,33 * * * *",
                        Feeds = [new RssFeedOptions { Name = "Home", Url = "https://aajtak.in/rss?id=home", Category = "General", Language = "hi", Enabled = true }],
                    },
                    new RssProviderOptions
                    {
                        Name = "ABPNews",
                        Enabled = true,
                        Cron = "0 9 * * *",
                        Feeds = [new RssFeedOptions { Name = "Home", Url = "https://abplive.com/feed", Category = "General", Language = "hi", Enabled = true }],
                    },
                ],
            },
        ],
    };

    private static NewsApiCrawlerOptions BuildApiOptions() => new()
    {
        Countries =
        [
            new NewsApiCountryOptions
            {
                Name = "India",
                Enabled = true,
                Providers =
                [
                    new NewsApiProviderOptions
                    {
                        Name = "NewsApiOrg",
                        Enabled = true,
                        Cron = "17 3 * * *",
                        BaseUrl = "https://newsapi.org/v2",
                        AuthType = ApiAuthType.QueryParameter,
                        AuthParamName = "apiKey",
                        TimeoutSeconds = 30,
                        Endpoints = [new NewsApiEndpointOptions { Name = "Everything", Endpoint = "everything", Category = "General", Language = "en", Enabled = true }],
                    },
                ],
            },
        ],
    };

    private static ProviderSchedule BuildLegacySchedule(CrawlPipeline pipeline, string provider) => new()
    {
        Id = $"{pipeline}-{provider}",
        Pipeline = pipeline,
        Provider = provider,
        Country = "India",
        Enabled = true,
        Cron = "*/20 * * * *",
        TimeZone = "UTC",
        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-30),
    };

    [Fact]
    public async Task UpgradeLegacyDefaultCronsAsync_DocumentStillOnLegacyCron_IsUpgradedToConfiguredCron()
    {
        var scheduleRepo = new Mock<IProviderScheduleRepository>();
        var rssLegacy = BuildLegacySchedule(CrawlPipeline.Rss, "AajTak");
        var apiLegacy = BuildLegacySchedule(CrawlPipeline.Api, "NewsApiOrg");

        scheduleRepo.Setup(s => s.GetAllAsync(CrawlPipeline.Rss, It.IsAny<CancellationToken>())).ReturnsAsync([rssLegacy]);
        scheduleRepo.Setup(s => s.GetAllAsync(CrawlPipeline.Api, It.IsAny<CancellationToken>())).ReturnsAsync([apiLegacy]);

        ProviderSchedule? upsertedRss = null;
        ProviderSchedule? upsertedApi = null;
        scheduleRepo
            .Setup(s => s.UpsertAsync(It.Is<ProviderSchedule>(p => p.Pipeline == CrawlPipeline.Rss), It.IsAny<CancellationToken>()))
            .Callback<ProviderSchedule, CancellationToken>((s, _) => upsertedRss = s)
            .Returns(Task.CompletedTask);
        scheduleRepo
            .Setup(s => s.UpsertAsync(It.Is<ProviderSchedule>(p => p.Pipeline == CrawlPipeline.Api), It.IsAny<CancellationToken>()))
            .Callback<ProviderSchedule, CancellationToken>((s, _) => upsertedApi = s)
            .Returns(Task.CompletedTask);

        var seeder = new ProviderScheduleSeeder(
            scheduleRepo.Object, Options.Create(BuildRssOptions()), Options.Create(BuildApiOptions()), NullLogger<ProviderScheduleSeeder>.Instance);

        await seeder.UpgradeLegacyDefaultCronsAsync(CancellationToken.None);

        Assert.NotNull(upsertedRss);
        Assert.Equal("3,33 * * * *", upsertedRss!.Cron);

        Assert.NotNull(upsertedApi);
        Assert.Equal("17 3 * * *", upsertedApi!.Cron);
    }

    [Fact]
    public async Task UpgradeLegacyDefaultCronsAsync_DocumentAlreadyDivergedFromLegacyCron_IsLeftUntouched()
    {
        var scheduleRepo = new Mock<IProviderScheduleRepository>();
        var userEdited = BuildLegacySchedule(CrawlPipeline.Rss, "AajTak");
        userEdited.Cron = "*/5 * * * *"; // a user already changed this away from the legacy default

        scheduleRepo.Setup(s => s.GetAllAsync(CrawlPipeline.Rss, It.IsAny<CancellationToken>())).ReturnsAsync([userEdited]);
        scheduleRepo.Setup(s => s.GetAllAsync(CrawlPipeline.Api, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var seeder = new ProviderScheduleSeeder(
            scheduleRepo.Object, Options.Create(BuildRssOptions()), Options.Create(BuildApiOptions()), NullLogger<ProviderScheduleSeeder>.Instance);

        await seeder.UpgradeLegacyDefaultCronsAsync(CancellationToken.None);

        scheduleRepo.Verify(s => s.UpsertAsync(It.IsAny<ProviderSchedule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpgradeLegacyDefaultCronsAsync_ProviderNoLongerInConfig_IsLeftUntouched()
    {
        var scheduleRepo = new Mock<IProviderScheduleRepository>();
        var orphaned = BuildLegacySchedule(CrawlPipeline.Rss, "SomeRemovedProvider");

        scheduleRepo.Setup(s => s.GetAllAsync(CrawlPipeline.Rss, It.IsAny<CancellationToken>())).ReturnsAsync([orphaned]);
        scheduleRepo.Setup(s => s.GetAllAsync(CrawlPipeline.Api, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var seeder = new ProviderScheduleSeeder(
            scheduleRepo.Object, Options.Create(BuildRssOptions()), Options.Create(BuildApiOptions()), NullLogger<ProviderScheduleSeeder>.Instance);

        await seeder.UpgradeLegacyDefaultCronsAsync(CancellationToken.None);

        scheduleRepo.Verify(s => s.UpsertAsync(It.IsAny<ProviderSchedule>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
