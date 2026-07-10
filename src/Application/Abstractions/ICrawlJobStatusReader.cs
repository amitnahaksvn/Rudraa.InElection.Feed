using Application.Models;
using Domain.Enums;

namespace Application.Abstractions;

public interface ICrawlJobStatusReader
{
    /// <summary>RSS and API providers register their recurring job under different id schemes ("news-crawl-{provider}" vs "news-api-{provider}") - <paramref name="pipeline"/> picks which one to look up. Social has no recurring per-source job status reader today.</summary>
    /// <returns>Null if no recurring job is registered for that provider.</returns>
    CrawlJobStatus? GetStatus(CrawlPipeline pipeline, string providerName);

    /// <summary>
    /// Batched counterpart to <see cref="GetStatus"/> - the crawl-report page needs one provider's
    /// status per row, and a provider-at-a-time loop over <see cref="GetStatus"/> means one
    /// Hangfire/Mongo round trip per provider (236+ RSS providers alone), which measured out to
    /// ~58 seconds end to end. This resolves every requested provider's status in a single
    /// underlying query instead.
    /// </summary>
    /// <returns>Keyed by provider name; a provider with no registered recurring job is simply absent from the result.</returns>
    IReadOnlyDictionary<string, CrawlJobStatus> GetStatuses(CrawlPipeline pipeline, IReadOnlyCollection<string> providerNames);
}
