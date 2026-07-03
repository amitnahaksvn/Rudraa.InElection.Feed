using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Application.Abstractions;

namespace Infrastructure.Scheduling;

/// <summary>
/// The one thing every <see cref="Domain.Entities.FeedSource"/>-driven Hangfire job actually
/// invokes - never <see cref="IDynamicFeedIngestionService"/> directly - same reasoning as
/// <see cref="HangfireCrawlJobExecutor"/>: a friendly dashboard name and every log line tagged
/// with this specific job execution's id via <see cref="PerformContext"/>.
///
/// Tagged onto the same "rss" queue as the file-configured providers (dynamic feeds are the same
/// kind of work - scheduled RSS polling - so they share the same replica-scaling group by
/// default); a future news-API-fetching executor would get its own "api" queue instead.
/// </summary>
[Queue("rss")]
public sealed class HangfireDynamicFeedJobExecutor
{
    private readonly IDynamicFeedIngestionService _ingestionService;
    private readonly ILogger<HangfireDynamicFeedJobExecutor> _logger;

    public HangfireDynamicFeedJobExecutor(IDynamicFeedIngestionService ingestionService, ILogger<HangfireDynamicFeedJobExecutor> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    [JobDisplayName("Ingest feed {0}")]
    public async Task RunAsync(string feedSourceId, PerformContext context, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["HangfireJobId"] = context.BackgroundJob.Id,
            ["FeedSourceId"] = feedSourceId
        });

        await _ingestionService.RunAsync(feedSourceId, cancellationToken);
    }
}
