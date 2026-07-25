using Domain.Enums;

namespace Application.Models;

/// <summary>Query shape for <see cref="Abstractions.ICrawlHistoryRepository.GetFilteredAsync"/> - every filter is optional/additive (null/default = "don't filter on this"), so a caller can ask for anything from "the last 20 runs of anything" to "every AajTak run between two dates". <see cref="Providers"/> is additive to <see cref="Provider"/> (matches a run if ANY of its own <c>Providers</c> list is in this set) - used by the crawl-report page's multi-provider filter, distinct from the single-provider exact match.</summary>
public sealed record CrawlHistoryFilter(
    CrawlPipeline? Pipeline = null,
    string? Provider = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Skip = 0,
    int Take = 20,
    IReadOnlyList<string>? Providers = null);
