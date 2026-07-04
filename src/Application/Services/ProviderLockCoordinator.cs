using Microsoft.Extensions.Logging;
using Application.Abstractions;

namespace Application.Services;

/// <summary>
/// Shared per-provider distributed-lock acquire/release logic used by every crawler orchestrator -
/// <see cref="NewsCrawlerOrchestrator"/> (RSS) and <see cref="NewsApiCrawlerOrchestrator"/> (JSON
/// APIs) alike. Locking is deliberately per-provider, not global (see either orchestrator's own
/// doc comment for why), so this exists once instead of two near-identical copies of the same
/// acquire-loop/release-loop.
/// </summary>
internal static class ProviderLockCoordinator
{
    /// <summary>
    /// Attempts to acquire the named lock for every candidate, skipping (and logging) any whose
    /// lock is already held elsewhere. Returns only the providers that were actually acquired.
    /// </summary>
    public static async Task<IReadOnlyList<TProviderOptions>> AcquireAsync<TProviderOptions>(
        IReadOnlyList<TProviderOptions> candidates,
        Func<TProviderOptions, string> getName,
        Func<string, string> getLockName,
        ICrawlLockRepository lockRepository,
        string ownerId,
        TimeSpan lockTtl,
        ILogger logger,
        string skippedLogMessage,
        CancellationToken cancellationToken)
    {
        var locked = new List<TProviderOptions>();

        foreach (var candidate in candidates)
        {
            var name = getName(candidate);
            var lockName = getLockName(name);

            if (await lockRepository.TryAcquireAsync(lockName, ownerId, lockTtl, cancellationToken))
            {
                locked.Add(candidate);
            }
            else
            {
                logger.LogInformation(skippedLogMessage, name, lockName);
            }
        }

        return locked;
    }

    public static async Task ReleaseAsync<TProviderOptions>(
        IReadOnlyList<TProviderOptions> lockedProviders,
        Func<TProviderOptions, string> getName,
        Func<string, string> getLockName,
        ICrawlLockRepository lockRepository,
        string ownerId,
        CancellationToken cancellationToken)
    {
        foreach (var provider in lockedProviders)
        {
            await lockRepository.ReleaseAsync(getLockName(getName(provider)), ownerId, cancellationToken);
        }
    }
}
