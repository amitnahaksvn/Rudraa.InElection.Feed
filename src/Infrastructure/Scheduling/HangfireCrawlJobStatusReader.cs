using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Caching.Memory;
using Application.Abstractions;
using Application.Models;
using Domain.Enums;

namespace Infrastructure.Scheduling;

public sealed class HangfireCrawlJobStatusReader : ICrawlJobStatusReader
{
    // Same concurrency this codebase already settled on for the identical problem shape in
    // HangfireRecurringJobRegistrar (see its own doc comment) - one Hangfire/Mongo round trip per
    // job id is unavoidable (StorageConnectionExtensions.GetRecurringJobs iterates ids one at a
    // time internally, calling GetAllEntriesFromHash - and, for a failed last run,
    // JobDetails - even when given every id in a single call), so the only lever is running many
    // of those round trips concurrently instead of sequentially.
    private const int LookupConcurrency = 64;

    // How long one provider's schedule snapshot is trusted before re-fetching. A recurring job's
    // Cron/NextExecution/LastExecution/LastJobState change at most once per that provider's own
    // cron tick (the shortest configured is every few minutes) - caching for this short a window
    // keeps the crawl-report page feeling instant on a second load (tab switch, date-range tweak)
    // without ever showing meaningfully stale schedule data.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(20);

    private readonly JobStorage _jobStorage;
    private readonly IMemoryCache _cache;

    /// <summary>See HangfireRecurringJobRegistrar's identical static constructor for why this exists: Parallel.ForEach dispatches blocking calls onto the CLR ThreadPool, which only grows slowly under its own throttled "hill-climbing" algorithm when starved - raising the floor once, up front, means the concurrency this class asks for is actually available rather than queuing behind a cold pool.</summary>
    static HangfireCrawlJobStatusReader()
    {
        ThreadPool.GetMinThreads(out _, out var completionPortMin);
        ThreadPool.SetMinThreads(LookupConcurrency * 2, completionPortMin);
    }

    public HangfireCrawlJobStatusReader(JobStorage jobStorage, IMemoryCache cache)
    {
        _jobStorage = jobStorage;
        _cache = cache;
    }

    public CrawlJobStatus? GetStatus(CrawlPipeline pipeline, string providerName) =>
        GetStatuses(pipeline, [providerName]).GetValueOrDefault(providerName);

    /// <summary>
    /// A naive per-provider loop here measured ~58-65s end to end against this app's 236 RSS
    /// providers (network round trips to a remote Atlas cluster, not CPU) - unacceptable for a
    /// page a person is actively waiting on. Fanning the same per-provider lookup out across
    /// <see cref="LookupConcurrency"/> concurrent connections, plus the short cache above, cut
    /// that down to roughly a couple of seconds on a cold cache and near-instant on a warm one.
    /// </summary>
    public IReadOnlyDictionary<string, CrawlJobStatus> GetStatuses(CrawlPipeline pipeline, IReadOnlyCollection<string> providerNames)
    {
        if (providerNames.Count == 0)
        {
            return new Dictionary<string, CrawlJobStatus>();
        }

        var result = new ConcurrentDictionary<string, CrawlJobStatus>();
        var uncached = new List<string>();

        foreach (var providerName in providerNames)
        {
            if (_cache.TryGetValue(CacheKey(pipeline, providerName), out CacheEntry entry))
            {
                if (entry.Status is not null)
                {
                    result[providerName] = entry.Status;
                }
            }
            else
            {
                uncached.Add(providerName);
            }
        }

        Parallel.ForEach(uncached, new ParallelOptions { MaxDegreeOfParallelism = LookupConcurrency }, providerName =>
        {
            var status = GetStatusUncached(pipeline, providerName);
            _cache.Set(CacheKey(pipeline, providerName), new CacheEntry(status), CacheDuration);
            if (status is not null)
            {
                result[providerName] = status;
            }
        });

        return result;
    }

    private static string CacheKey(CrawlPipeline pipeline, string providerName) => $"crawl-job-status:{pipeline}:{providerName}";

    private readonly record struct CacheEntry(CrawlJobStatus? Status);

    private CrawlJobStatus? GetStatusUncached(CrawlPipeline pipeline, string providerName)
    {
        var jobId = pipeline == CrawlPipeline.Api
            ? HangfireJobIds.NewsApi(providerName)
            : HangfireJobIds.NewsCrawl(providerName);

        using var connection = _jobStorage.GetConnection();
        var recurringJob = connection.GetRecurringJobs(new[] { jobId }).SingleOrDefault();

        if (recurringJob is null)
        {
            return null;
        }

        string? lastErrorType = null;
        string? lastErrorMessage = null;

        if (recurringJob.LastJobState == "Failed" && !string.IsNullOrEmpty(recurringJob.LastJobId))
        {
            var details = _jobStorage.GetMonitoringApi().JobDetails(recurringJob.LastJobId);
            var failedState = details?.History?.FirstOrDefault(h => h.StateName == "Failed");
            if (failedState is not null)
            {
                failedState.Data.TryGetValue("ExceptionType", out lastErrorType);
                failedState.Data.TryGetValue("ExceptionMessage", out lastErrorMessage);
            }
        }
        else if (!string.IsNullOrEmpty(recurringJob.Error))
        {
            // A recurring-job-level error (e.g. an invalid cron expression) rather than a
            // failure of a specific run.
            lastErrorMessage = recurringJob.Error;
        }

        return new CrawlJobStatus(
            JobId: jobId,
            Provider: providerName,
            Cron: recurringJob.Cron,
            TimeZone: recurringJob.TimeZoneId,
            NextExecution: ToUtcOffset(recurringJob.NextExecution),
            LastExecution: ToUtcOffset(recurringJob.LastExecution),
            LastJobId: recurringJob.LastJobId,
            LastJobState: recurringJob.LastJobState,
            LastErrorType: lastErrorType,
            LastErrorMessage: lastErrorMessage);
    }

    private static DateTimeOffset? ToUtcOffset(DateTime? value) =>
        value.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)) : null;
}
