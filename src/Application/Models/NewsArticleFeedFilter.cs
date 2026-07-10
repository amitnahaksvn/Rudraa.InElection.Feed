using Domain.Enums;

namespace Application.Models;

/// <summary>Query shape for <see cref="Abstractions.INewsArticleRepository.GetFeedAsync"/> - backs the News Feed page's infinite scroll: narrow by pipeline (RSS/API) and/or country, page via Skip/Take, newest first.</summary>
public sealed record NewsArticleFeedFilter(
    ArticleSourceType? SourceType = null,
    string? Country = null,
    int Skip = 0,
    int Take = 20);
