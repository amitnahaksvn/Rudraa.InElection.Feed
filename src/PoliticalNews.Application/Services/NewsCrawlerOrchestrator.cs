using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Application.Models;
using PoliticalNews.Application.Options;
using PoliticalNews.Domain.Entities;
using PoliticalNews.Domain.Enums;

namespace PoliticalNews.Application.Services;

/// <summary>
/// Coordinates a full crawl run: for every enabled provider/feed in configuration, fetches and
/// normalizes articles via <see cref="IRssProvider"/>, then deduplicates and persists them via
/// <see cref="INewsArticleRepository"/>. Contains no MongoDB or HTTP/XML specifics itself.
/// </summary>
public sealed class NewsCrawlerOrchestrator : INewsCrawlerService
{
    private readonly IEnumerable<IRssProvider> _providers;
    private readonly INewsArticleRepository _articleRepository;
    private readonly ICrawlHistoryRepository _historyRepository;
    private readonly ICrawlLockRepository _lockRepository;
    private readonly IRssRawResponseRepository _rawResponseRepository;
    private readonly NewsCrawlerOptions _options;
    private readonly ILogger<NewsCrawlerOrchestrator> _logger;
    private readonly string _ownerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    public NewsCrawlerOrchestrator(
        IEnumerable<IRssProvider> providers,
        INewsArticleRepository articleRepository,
        ICrawlHistoryRepository historyRepository,
        ICrawlLockRepository lockRepository,
        IRssRawResponseRepository rawResponseRepository,
        IOptions<NewsCrawlerOptions> options,
        ILogger<NewsCrawlerOrchestrator> logger)
    {
        _providers = providers;
        _articleRepository = articleRepository;
        _historyRepository = historyRepository;
        _lockRepository = lockRepository;
        _rawResponseRepository = rawResponseRepository;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Runs a full crawl, guarded by a Mongo-backed distributed lock so that the scheduled
    /// worker tick and a manually triggered API call (or another instance entirely) can never
    /// execute concurrently. Returns a non-persisted <see cref="CrawlStatus.Skipped"/> result
    /// if another run already holds the lock.
    /// </summary>
    public Task<CrawlHistory> RunCrawlAsync(CancellationToken cancellationToken) =>
        RunCrawlAsync(providerFilter: null, cancellationToken);

    /// <inheritdoc cref="INewsCrawlerService.RunCrawlAsync(IReadOnlyCollection{string}, CancellationToken)" />
    public Task<CrawlHistory> RunCrawlAsync(IReadOnlyCollection<string> providerNames, CancellationToken cancellationToken) =>
        RunCrawlAsync(p => providerNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase), cancellationToken);

    private async Task<CrawlHistory> RunCrawlAsync(Func<RssProviderOptions, bool>? providerFilter, CancellationToken cancellationToken)
    {
        var acquired = await _lockRepository.TryAcquireAsync(
            _options.LockName, _ownerId, _options.LockTtl, cancellationToken);

        if (!acquired)
        {
            _logger.LogInformation("Crawl skipped - lock '{Lock}' is held by another run", _options.LockName);
            var now = DateTimeOffset.UtcNow;
            return new CrawlHistory { StartTime = now, EndTime = now, Duration = TimeSpan.Zero, Status = CrawlStatus.Skipped };
        }

        try
        {
            return await RunLockedAsync(providerFilter, cancellationToken);
        }
        finally
        {
            await _lockRepository.ReleaseAsync(_options.LockName, _ownerId, CancellationToken.None);
        }
    }

    private async Task<CrawlHistory> RunLockedAsync(Func<RssProviderOptions, bool>? providerFilter, CancellationToken cancellationToken)
    {
        var history = new CrawlHistory
        {
            StartTime = DateTimeOffset.UtcNow,
            Status = CrawlStatus.Running
        };
        history.Id = await _historyRepository.InsertAsync(history, cancellationToken);

        var failedFeeds = new List<string>();
        var newCount = 0;
        var updatedCount = 0;
        var duplicateCount = 0;
        var feedCount = 0;
        string? runError = null;

        try
        {
            foreach (var providerOptions in _options.Providers.Where(p => p.Enabled && (providerFilter is null || providerFilter(p))))
            {
                var provider = _providers.FirstOrDefault(p =>
                    string.Equals(p.Name, providerOptions.Name, StringComparison.OrdinalIgnoreCase));

                if (provider is null)
                {
                    _logger.LogWarning(
                        "No IRssProvider registered for configured provider '{Provider}' - skipping",
                        providerOptions.Name);
                    continue;
                }

                var enabledFeeds = providerOptions.Feeds.Where(f => f.Enabled).ToList();
                if (enabledFeeds.Count == 0)
                {
                    continue;
                }

                _logger.LogInformation("{Provider} RSS feed task started ({FeedCount} feeds)", provider.Name, enabledFeeds.Count);
                var providerStopwatch = Stopwatch.StartNew();
                var providerNewCount = 0;
                var providerUpdatedCount = 0;
                var providerDuplicateCount = 0;
                var providerFailedCount = 0;

                foreach (var feed in enabledFeeds)
                {
                    _logger.LogInformation("Feed started: {Provider}/{Feed} ({Url})", provider.Name, feed.Name, feed.Url);
                }

                var results = await provider.FetchAllFeedsAsync(enabledFeeds, cancellationToken);
                feedCount += results.Count;

                var saveRawResponses = _options.SaveRawResponses && providerOptions.SaveRawResponses;

                foreach (var result in results)
                {
                    if (saveRawResponses)
                    {
                        await _rawResponseRepository.InsertAsync(
                            new RssRawResponse
                            {
                                Provider = provider.Name,
                                FeedName = result.FeedName,
                                FeedUrl = result.FeedUrl,
                                FetchedAt = result.FetchedAt,
                                HttpStatusCode = result.HttpStatusCode,
                                RawXml = result.RawXml,
                                ContentHash = result.ContentHash,
                                ParseSucceeded = result.Success,
                                ParseError = result.Error,
                                ProcessingDurationMs = result.ProcessingDurationMs,
                                CreatedAt = DateTimeOffset.UtcNow
                            },
                            cancellationToken);
                    }

                    if (!result.Success)
                    {
                        failedFeeds.Add($"{provider.Name}/{result.FeedName}");
                        providerFailedCount++;
                        _logger.LogError("Feed failed: {Provider}/{Feed} - {Error}", provider.Name, result.FeedName, result.Error);
                        continue;
                    }

                    var (inserted, updated, duplicates) = await PersistArticlesAsync(
                        result.Articles.Take(_options.BatchSize),
                        cancellationToken);

                    newCount += inserted;
                    updatedCount += updated;
                    duplicateCount += duplicates;
                    providerNewCount += inserted;
                    providerUpdatedCount += updated;
                    providerDuplicateCount += duplicates;

                    _logger.LogInformation(
                        "Feed completed: {Provider}/{Feed} - {New} new, {Updated} updated, {Duplicate} duplicates",
                        provider.Name, result.FeedName, inserted, updated, duplicates);
                }

                providerStopwatch.Stop();
                _logger.LogInformation(
                    "{Provider} RSS feed task completed in {Elapsed} - {New} new, {Updated} updated, {Duplicate} duplicates, {Failed} failed feeds",
                    provider.Name, providerStopwatch.Elapsed, providerNewCount, providerUpdatedCount, providerDuplicateCount, providerFailedCount);
            }

            history.Status = failedFeeds.Count == 0 ? CrawlStatus.Completed : CrawlStatus.CompletedWithErrors;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Same reasoning as BaseRssProvider: a stray OperationCanceledException from some
            // inner timeout (not our own token) must be recorded as a failed run, not crash the
            // caller. Only propagates uncaught when cancellationToken itself was cancelled.
            runError = ex.Message;
            history.Status = CrawlStatus.Failed;
            _logger.LogError(ex, "Crawl run {HistoryId} failed unexpectedly", history.Id);
        }

        history.EndTime = DateTimeOffset.UtcNow;
        history.Duration = history.EndTime - history.StartTime;
        history.FeedCount = feedCount;
        history.NewArticles = newCount;
        history.UpdatedArticles = updatedCount;
        history.DuplicateArticles = duplicateCount;
        history.FailedFeeds = failedFeeds;
        history.Error = runError;

        await _historyRepository.UpdateAsync(history, cancellationToken);

        _logger.LogInformation(
            "Crawl completed: {Status} in {Duration} - {New} new, {Updated} updated, {Duplicate} duplicates, {Failed} failed feeds",
            history.Status, history.Duration, newCount, updatedCount, duplicateCount, failedFeeds.Count);

        return history;
    }

    private async Task<(int Inserted, int Updated, int Duplicates)> PersistArticlesAsync(
        IEnumerable<NormalizedArticle> articles,
        CancellationToken cancellationToken)
    {
        var inserted = 0;
        var updated = 0;
        var duplicates = 0;

        foreach (var normalized in articles)
        {
            var now = DateTimeOffset.UtcNow;
            var article = new NewsArticle
            {
                Provider = normalized.Provider,
                FeedName = normalized.FeedName,
                Category = normalized.Category,
                Title = normalized.Title,
                Summary = normalized.Summary,
                Content = normalized.Content,
                Url = normalized.Url,
                OriginalGuid = normalized.OriginalGuid,
                Author = normalized.Author,
                Language = normalized.Language,
                ImageUrl = normalized.ImageUrl,
                PublishedAt = normalized.PublishedAt,
                CrawledAt = now,
                UpdatedAt = now,
                Tags = normalized.Tags,
                Source = normalized.Source,
                Hash = ArticleHasher.ComputeHash(normalized.Title, normalized.PublishedAt),
                IsActive = true
            };

            var outcome = await _articleRepository.UpsertAsync(article, cancellationToken);
            switch (outcome)
            {
                case ArticleUpsertOutcome.Inserted:
                    inserted++;
                    _logger.LogInformation("New article inserted: {Title} ({Url})", article.Title, article.Url);
                    break;
                case ArticleUpsertOutcome.Updated:
                    updated++;
                    _logger.LogInformation("Existing article updated: {Title} ({Url})", article.Title, article.Url);
                    break;
                case ArticleUpsertOutcome.DuplicateSkipped:
                    duplicates++;
                    _logger.LogDebug("Duplicate skipped: {Title} ({Url})", article.Title, article.Url);
                    break;
            }
        }

        return (inserted, updated, duplicates);
    }
}
