using PoliticalNews.Domain.Entities;

namespace PoliticalNews.Application.Abstractions;

public interface ICrawlHistoryRepository
{
    /// <summary>Inserts a new history record (status = Running) and returns its generated Id.</summary>
    Task<string> InsertAsync(CrawlHistory history, CancellationToken cancellationToken);

    Task UpdateAsync(CrawlHistory history, CancellationToken cancellationToken);

    Task<IReadOnlyList<CrawlHistory>> GetRecentAsync(int count, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
