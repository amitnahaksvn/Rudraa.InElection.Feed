using Domain.Entities;

namespace Application.Abstractions;

/// <summary>Orchestrates one end-to-end news-API crawl run across every enabled provider. The <see cref="INewsCrawlerService"/> counterpart for JSON APIs.</summary>
public interface INewsApiCrawlerService
{
    /// <summary>Crawls every enabled news-API provider.</summary>
    Task<CrawlHistory> RunCrawlAsync(CancellationToken cancellationToken);

    /// <summary>Crawls only the given (still individually enabled) providers - used by the Hangfire executor, which fires per provider.</summary>
    Task<CrawlHistory> RunCrawlAsync(IReadOnlyCollection<string> providerNames, CancellationToken cancellationToken);
}
