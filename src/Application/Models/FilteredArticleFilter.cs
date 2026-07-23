using Domain.Enums;

namespace Application.Models;

/// <summary>
/// Query shape for <see cref="Abstractions.IFilteredArticleRepository.GetPagedAsync"/>/
/// <see cref="Abstractions.IFilteredArticleRepository.CountAsync"/> - narrows the Filtered
/// Articles admin page's table by provider/type/category. Always ordered newest-first
/// (PulledAt descending), the same "just-crawled" ordering the rest of this app's admin lists use.
/// </summary>
public sealed record FilteredArticleFilter(
    string? Provider = null,
    ArticleSourceType? SourceType = null,
    string? Category = null);
