using Cronos;
using Microsoft.Extensions.Options;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Application.Options;

namespace PoliticalNews.Worker.HostedServices;

/// <summary>
/// Drives the crawl schedule from the configured cron expression. Mutual exclusion (so a
/// scheduled tick never overlaps another tick, another worker instance, or a manually
/// triggered API crawl) is enforced inside <see cref="INewsCrawlerService"/> itself via a
/// distributed lock - this class only owns timing. A run that throws is logged and the
/// scheduler simply waits for its next tick rather than crashing the host.
/// </summary>
public sealed class CrawlerBackgroundService : BackgroundService
{
    private readonly INewsCrawlerService _crawlerService;
    private readonly NewsCrawlerOptions _options;
    private readonly ILogger<CrawlerBackgroundService> _logger;
    private readonly CronExpression _cronExpression;

    public CrawlerBackgroundService(
        INewsCrawlerService crawlerService,
        IOptions<NewsCrawlerOptions> options,
        ILogger<CrawlerBackgroundService> logger)
    {
        _crawlerService = crawlerService;
        _options = options.Value;
        _logger = logger;
        _cronExpression = CronExpression.Parse(_options.Cron, CronFormat.Standard);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("NewsCrawler is disabled via configuration ({Section}:Enabled=false) - scheduler will not run", NewsCrawlerOptions.SectionName);
            return;
        }

        _logger.LogInformation("Scheduler started. Cron '{Cron}', lock '{Lock}'", _options.Cron, _options.LockName);

        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
            if (next is null)
            {
                _logger.LogWarning("Cron expression '{Cron}' has no future occurrence - stopping scheduler", _options.Cron);
                break;
            }

            var delay = next.Value - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                _logger.LogInformation("Crawl run starting");
                await _crawlerService.RunCrawlAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Belt-and-braces: NewsCrawlerOrchestrator already turns feed failures/timeouts
                // into a Failed CrawlHistory instead of throwing, but if anything unexpected still
                // escapes it, this must never take the whole Worker process down with it - only a
                // genuine shutdown (stoppingToken cancelled) is allowed to propagate out of here.
                _logger.LogError(ex, "Crawl run threw an unhandled exception - scheduler continues on the next tick");
            }
        }

        _logger.LogInformation("Scheduler stopped (graceful shutdown)");
    }
}
