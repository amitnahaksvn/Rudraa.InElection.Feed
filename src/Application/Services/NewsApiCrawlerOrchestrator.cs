using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Abstractions;
using Application.Options;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Coordinates a full news-API crawl run: for every enabled provider in configuration, calls its
/// endpoint via <see cref="INewsApiProvider"/>, then deduplicates and persists the normalized
/// articles via <see cref="INewsArticleRepository"/> (the same repository/dedup path RSS uses).
/// The <see cref="NewsCrawlerOrchestrator"/> counterpart for JSON APIs - deliberately a separate
/// orchestrator/lock-namespace/options-section rather than folded into the RSS one, since the two
/// fetch shapes (a list of feeds per provider vs a single rate-limited endpoint per provider)
/// don't share a request loop even though they share everything downstream of "normalized article".
/// </summary>
public sealed class NewsApiCrawlerOrchestrator : INewsApiCrawlerService
{
    private readonly IEnumerable<INewsApiProvider> _providers;
    private readonly INewsArticleRepository _articleRepository;
    private readonly ICrawlHistoryRepository _historyRepository;
    private readonly ICrawlLockRepository _lockRepository;
    private readonly NewsApiCrawlerOptions _options;
    private readonly ILogger<NewsApiCrawlerOrchestrator> _logger;
    private readonly string _ownerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    public NewsApiCrawlerOrchestrator(
        IEnumerable<INewsApiProvider> providers,
        INewsArticleRepository articleRepository,
        ICrawlHistoryRepository historyRepository,
        ICrawlLockRepository lockRepository,
        IOptions<NewsApiCrawlerOptions> options,
        ILogger<NewsApiCrawlerOrchestrator> logger)
    {
        _providers = providers;
        _articleRepository = articleRepository;
        _historyRepository = historyRepository;
        _lockRepository = lockRepository;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Runs every enabled provider. Locking mirrors <see cref="NewsCrawlerOrchestrator"/>: per-provider, not global, so providers fetch in parallel across Hangfire ticks without starving each other.</summary>
    public Task<CrawlHistory> RunCrawlAsync(CancellationToken cancellationToken) =>
        RunCrawlAsync(providerFilter: null, cancellationToken);

    /// <inheritdoc cref="INewsApiCrawlerService.RunCrawlAsync(IReadOnlyCollection{string}, CancellationToken)" />
    public Task<CrawlHistory> RunCrawlAsync(IReadOnlyCollection<string> providerNames, CancellationToken cancellationToken) =>
        RunCrawlAsync(p => providerNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase), cancellationToken);

    private async Task<CrawlHistory> RunCrawlAsync(Func<NewsApiProviderOptions, bool>? providerFilter, CancellationToken cancellationToken)
    {
        var candidates = _options.Providers
            .Where(p => p.Enabled && (providerFilter is null || providerFilter(p)))
            .ToList();

        var lockedProviders = new List<NewsApiProviderOptions>();
        foreach (var provider in candidates)
        {
            if (await _lockRepository.TryAcquireAsync(ProviderLockName(provider.Name), _ownerId, _options.LockTtl, cancellationToken))
            {
                lockedProviders.Add(provider);
            }
            else
            {
                _logger.LogInformation(
                    "News API crawl skipped for provider {Provider} - lock '{Lock}' is held by another run",
                    provider.Name, ProviderLockName(provider.Name));
            }
        }

        if (lockedProviders.Count == 0)
        {
            var now = DateTimeOffset.UtcNow;
            return new CrawlHistory { StartTime = now, EndTime = now, Duration = TimeSpan.Zero, Status = CrawlStatus.Skipped };
        }

        try
        {
            return await RunLockedAsync(lockedProviders, cancellationToken);
        }
        finally
        {
            foreach (var provider in lockedProviders)
            {
                await _lockRepository.ReleaseAsync(ProviderLockName(provider.Name), _ownerId, CancellationToken.None);
            }
        }
    }

    private string ProviderLockName(string providerName) => $"{_options.LockName}:{providerName}";

    private async Task<CrawlHistory> RunLockedAsync(IReadOnlyList<NewsApiProviderOptions> lockedProviders, CancellationToken cancellationToken)
    {
        var history = new CrawlHistory
        {
            StartTime = DateTimeOffset.UtcNow,
            Status = CrawlStatus.Running
        };
        history.Id = await _historyRepository.InsertAsync(history, cancellationToken);

        var failedEndpoints = new List<string>();
        var newCount = 0;
        var updatedCount = 0;
        var duplicateCount = 0;
        var endpointCount = 0;
        string? runError = null;

        try
        {
            foreach (var providerOptions in lockedProviders)
            {
                var provider = _providers.FirstOrDefault(p =>
                    string.Equals(p.Name, providerOptions.Name, StringComparison.OrdinalIgnoreCase));

                if (provider is null)
                {
                    _logger.LogWarning(
                        "No INewsApiProvider registered for configured provider '{Provider}' - skipping",
                        providerOptions.Name);
                    continue;
                }

                var enabledEndpoints = providerOptions.Endpoints.Count(e => e.Enabled);
                if (enabledEndpoints == 0)
                {
                    continue;
                }

                _logger.LogInformation("[{RunId}] Started: {Provider} ({EndpointCount} endpoints)", history.Id, provider.Name, enabledEndpoints);
                var providerStopwatch = Stopwatch.StartNew();
                var providerNewCount = 0;
                var providerUpdatedCount = 0;
                var providerDuplicateCount = 0;
                var providerFailedCount = 0;

                var results = await provider.FetchAllEndpointsAsync(providerOptions, cancellationToken);
                endpointCount += results.Count;

                foreach (var result in results)
                {
                    if (!result.Success)
                    {
                        failedEndpoints.Add($"{provider.Name}/{result.EndpointName}");
                        providerFailedCount++;
                        _logger.LogError("News API endpoint failed: {Provider}/{Endpoint} - {Error}", provider.Name, result.EndpointName, result.Error);
                        continue;
                    }

                    var (inserted, updated, duplicates) = await ArticlePersister.PersistAsync(
                        _articleRepository,
                        result.Articles.Take(_options.BatchSize),
                        _logger,
                        cancellationToken);

                    newCount += inserted;
                    updatedCount += updated;
                    duplicateCount += duplicates;
                    providerNewCount += inserted;
                    providerUpdatedCount += updated;
                    providerDuplicateCount += duplicates;

                    _logger.LogDebug(
                        "News API endpoint completed: {Provider}/{Endpoint} - {New} new, {Updated} updated, {Duplicate} duplicates",
                        provider.Name, result.EndpointName, inserted, updated, duplicates);
                }

                providerStopwatch.Stop();
                _logger.LogInformation(
                    "[{RunId}] Completed: {Provider} - {New} new, {Updated} updated, {Duplicate} duplicate, {Failed} failed ({Elapsed})",
                    history.Id, provider.Name, providerNewCount, providerUpdatedCount, providerDuplicateCount, providerFailedCount, providerStopwatch.Elapsed);
            }

            history.Status = failedEndpoints.Count == 0 ? CrawlStatus.Completed : CrawlStatus.CompletedWithErrors;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            runError = ex.Message;
            history.Status = CrawlStatus.Failed;
            _logger.LogError(ex, "[{RunId}] News API crawl run failed unexpectedly", history.Id);
        }

        history.EndTime = DateTimeOffset.UtcNow;
        history.Duration = history.EndTime - history.StartTime;
        history.FeedCount = endpointCount;
        history.NewArticles = newCount;
        history.UpdatedArticles = updatedCount;
        history.DuplicateArticles = duplicateCount;
        history.FailedFeeds = failedEndpoints;
        history.Error = runError;

        await _historyRepository.UpdateAsync(history, cancellationToken);

        _logger.LogInformation(
            "[{RunId}] Crawl completed: {Status} - {New} new, {Updated} updated, {Duplicate} duplicate, {Failed} failed ({Duration})",
            history.Id, history.Status, newCount, updatedCount, duplicateCount, failedEndpoints.Count, history.Duration);

        return history;
    }
}
