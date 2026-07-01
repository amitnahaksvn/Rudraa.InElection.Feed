using PoliticalNews.Domain.Entities;

namespace PoliticalNews.Application.Abstractions;

/// <summary>Orchestrates one end-to-end crawl run across every enabled provider/feed.</summary>
public interface INewsCrawlerService
{
    Task<CrawlHistory> RunCrawlAsync(CancellationToken cancellationToken);
}
