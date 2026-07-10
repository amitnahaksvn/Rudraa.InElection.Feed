namespace Domain.Enums;

/// <summary>
/// Which fetch pipeline produced a <see cref="Entities.CrawlHistory"/> record - RSS
/// (<c>NewsCrawlerOrchestrator</c>), JSON news-API (<c>NewsApiCrawlerOrchestrator</c>), or Social
/// (<c>SocialMediaIngestionService</c>). All three share one <c>CrawlHistory</c> collection since
/// they share the same run/duration/new-updated-duplicate shape; this is the discriminator that
/// lets a report tell them apart without guessing from provider name conventions.
/// </summary>
public enum CrawlPipeline
{
    Rss,
    Api,
    Social
}
