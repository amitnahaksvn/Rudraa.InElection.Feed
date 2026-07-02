using Cronos;
using Microsoft.Extensions.Options;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Application.Options;

namespace PoliticalNews.Worker.HostedServices;

/// <summary>
/// Drives the crawl schedule - each provider has its own independent cron expression (<see
/// cref="RssProviderOptions.Cron"/>), so a 1-minute base tick (cron's own finest resolution) is
/// used to check every provider's schedule and only the providers due that minute are crawled.
/// Mutual exclusion (so a tick never overlaps another tick, another worker instance, or a
/// manually triggered API crawl) is enforced inside <see cref="INewsCrawlerService"/> itself via
/// a distributed lock - this class only owns timing. A run that throws is logged and the
/// scheduler simply waits for its next tick rather than crashing the host.
/// </summary>
public sealed class CrawlerBackgroundService : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    private readonly INewsCrawlerService _crawlerService;
    private readonly NewsCrawlerOptions _options;
    private readonly ILogger<CrawlerBackgroundService> _logger;
    private readonly Dictionary<string, CronExpression> _providerCrons;

    public CrawlerBackgroundService(
        INewsCrawlerService crawlerService,
        IOptions<NewsCrawlerOptions> options,
        ILogger<CrawlerBackgroundService> logger)
    {
        _crawlerService = crawlerService;
        _options = options.Value;
        _logger = logger;
        _providerCrons = BuildProviderCrons(_options, logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("NewsCrawler is disabled via configuration ({Section}:Enabled=false) - scheduler will not run", NewsCrawlerOptions.SectionName);
            return;
        }

        if (_providerCrons.Count == 0)
        {
            _logger.LogWarning("No provider has a valid Cron configured - scheduler will not run");
            return;
        }

        _logger.LogInformation(
            "Scheduler started. Providers: {Providers}, lock '{Lock}'",
            string.Join(", ", _providerCrons.Select(p => $"{p.Key}='{p.Value}'")), _options.LockName);

        var lastTick = DateTimeOffset.UtcNow;

        using var timer = new PeriodicTimer(TickInterval);
        while (true)
        {
            bool ticked;
            try
            {
                ticked = await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!ticked)
            {
                break;
            }

            var now = DateTimeOffset.UtcNow;
            var dueProviders = _providerCrons
                .Where(p => IsDue(p.Value, lastTick, now))
                .Select(p => p.Key)
                .ToList();
            lastTick = now;

            if (dueProviders.Count == 0)
            {
                continue;
            }

            try
            {
                _logger.LogInformation("Crawl run starting for providers: {Providers}", string.Join(", ", dueProviders));
                await _crawlerService.RunCrawlAsync(dueProviders, stoppingToken);
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

    /// <summary>True if <paramref name="cron"/> has an occurrence in the (<paramref name="lastTick"/>, <paramref name="now"/>] window.</summary>
    public static bool IsDue(CronExpression cron, DateTimeOffset lastTick, DateTimeOffset now)
    {
        var next = cron.GetNextOccurrence(lastTick, TimeZoneInfo.Utc);
        return next.HasValue && next.Value <= now;
    }

    private static Dictionary<string, CronExpression> BuildProviderCrons(NewsCrawlerOptions options, ILogger logger)
    {
        var result = new Dictionary<string, CronExpression>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in options.Providers.Where(p => p.Enabled))
        {
            if (string.IsNullOrWhiteSpace(provider.Cron))
            {
                logger.LogWarning("Provider '{Provider}' has no Cron configured - it will never run on schedule", provider.Name);
                continue;
            }

            try
            {
                result[provider.Name] = CronExpression.Parse(provider.Cron, CronFormat.Standard);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Provider '{Provider}' has an invalid Cron expression '{Cron}' - it will never run on schedule", provider.Name, provider.Cron);
            }
        }

        return result;
    }
}
