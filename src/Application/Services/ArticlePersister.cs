using Microsoft.Extensions.Logging;
using Application.Abstractions;
using Application.Models;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Shared normalized-article persistence logic (hash computation + upsert + outcome logging) used
/// by every crawler orchestrator - <see cref="NewsCrawlerOrchestrator"/> (RSS) and
/// <see cref="NewsApiCrawlerOrchestrator"/> (JSON APIs) alike - so the dedup/upsert path only
/// exists once regardless of how an article was fetched.
/// </summary>
internal static class ArticlePersister
{
    public static async Task<(int Inserted, int Updated, int Duplicates)> PersistAsync(
        INewsArticleRepository articleRepository,
        IEnumerable<NormalizedArticle> articles,
        ILogger logger,
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
                SourceType = normalized.SourceType,
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

            var outcome = await articleRepository.UpsertAsync(article, cancellationToken);
            switch (outcome)
            {
                case ArticleUpsertOutcome.Inserted:
                    inserted++;
                    logger.LogDebug("New article inserted: {Title} ({Url})", article.Title, article.Url);
                    break;
                case ArticleUpsertOutcome.Updated:
                    updated++;
                    logger.LogDebug("Existing article updated: {Title} ({Url})", article.Title, article.Url);
                    break;
                case ArticleUpsertOutcome.DuplicateSkipped:
                    duplicates++;
                    logger.LogDebug("Duplicate skipped: {Title} ({Url})", article.Title, article.Url);
                    break;
            }
        }

        return (inserted, updated, duplicates);
    }
}
