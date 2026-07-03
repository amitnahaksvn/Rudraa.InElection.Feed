namespace Application.Services;

/// <summary>
/// Ambient (<see cref="AsyncLocal{T}"/>) holder for the current Hangfire job id. Set by
/// <c>HangfireCrawlJobExecutor</c>/<c>HangfireNewsApiJobExecutor</c> right before invoking
/// <c>INewsCrawlerService</c>/<c>INewsApiCrawlerService</c>, and read by the orchestrators when
/// building an <see cref="Models.ErrorNotification"/> - avoids threading an extra parameter
/// through those interfaces' public signature just for this, which matters because they're also
/// invoked with no Hangfire job at all (Web's manual <c>POST /api/crawl/trigger</c>). Naturally
/// scoped per logical call chain - one Hangfire job execution's value never leaks into a
/// concurrently-running one, the same guarantee <see cref="AsyncLocal{T}"/> already gives
/// <c>ILogger.BeginScope</c>.
/// </summary>
public static class ExecutionContextAccessor
{
    private static readonly AsyncLocal<string?> HangfireJobIdLocal = new();

    public static string? CurrentHangfireJobId
    {
        get => HangfireJobIdLocal.Value;
        set => HangfireJobIdLocal.Value = value;
    }
}
