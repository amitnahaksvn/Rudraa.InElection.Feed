using Domain.Enums;

namespace Application.Models;

/// <summary>Query shape for <see cref="Abstractions.ICrawlHistoryRepository.GetFilteredAsync"/> - every filter is optional/additive (null/default = "don't filter on this"), so a caller can ask for anything from "the last 20 runs of anything" to "every AajTak run between two dates".</summary>
public sealed record CrawlHistoryFilter(
    CrawlPipeline? Pipeline = null,
    string? Provider = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Skip = 0,
    int Take = 20);
