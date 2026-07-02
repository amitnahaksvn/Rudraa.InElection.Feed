using Cronos;
using PoliticalNews.Worker.HostedServices;

namespace PoliticalNews.Tests.Worker;

public class CrawlerBackgroundServiceTests
{
    private static readonly DateTimeOffset LastTick = new(2026, 7, 2, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsDue_OccurrenceFallsWithinWindow_ReturnsTrue()
    {
        var cron = CronExpression.Parse("*/5 * * * *", CronFormat.Standard);
        var now = LastTick.AddMinutes(5);

        Assert.True(CrawlerBackgroundService.IsDue(cron, LastTick, now));
    }

    [Fact]
    public void IsDue_NoOccurrenceWithinWindow_ReturnsFalse()
    {
        var cron = CronExpression.Parse("*/5 * * * *", CronFormat.Standard);
        var now = LastTick.AddMinutes(1);

        Assert.False(CrawlerBackgroundService.IsDue(cron, LastTick, now));
    }

    [Fact]
    public void IsDue_DifferentProviderSchedules_FireIndependently()
    {
        var everyFiveMinutes = CronExpression.Parse("*/5 * * * *", CronFormat.Standard);
        var everyTenMinutes = CronExpression.Parse("*/10 * * * *", CronFormat.Standard);
        var now = LastTick.AddMinutes(5);

        Assert.True(CrawlerBackgroundService.IsDue(everyFiveMinutes, LastTick, now));
        Assert.False(CrawlerBackgroundService.IsDue(everyTenMinutes, LastTick, now));
    }
}
