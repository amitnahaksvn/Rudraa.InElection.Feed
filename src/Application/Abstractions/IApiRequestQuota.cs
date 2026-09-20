namespace Application.Abstractions;

/// <summary>
/// Shared (Mongo-backed, so it holds across processes/replicas) per-provider daily request
/// budget for JSON news APIs on a free tier.
/// </summary>
public interface IApiRequestQuota
{
    /// <summary>
    /// Atomically consumes one request from <paramref name="provider"/>'s budget for the current
    /// UTC day. Returns false, consuming nothing, once <paramref name="dailyLimit"/> is reached.
    /// </summary>
    Task<bool> TryConsumeAsync(string provider, int dailyLimit, CancellationToken cancellationToken);
}
