using MongoDB.Driver;
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class FilteredArticleRepository : IFilteredArticleRepository
{
    private readonly IMongoCollection<FilteredArticle> _collection;
    private readonly IArticleFingerprintRepository _fingerprints;

    public FilteredArticleRepository(MongoDbContext context, IArticleFingerprintRepository fingerprints)
    {
        _collection = context.FilteredArticles;
        _fingerprints = fingerprints;
    }

    public async Task InsertAsync(
        FilteredArticle article,
        string url,
        string? originalGuid,
        string hash,
        DateTimeOffset? publishedAt,
        CancellationToken cancellationToken)
    {
        var fingerprint = new ArticleFingerprint
        {
            Provider = article.Provider,
            SourceType = article.SourceType,
            Url = url,
            OriginalGuid = originalGuid,
            Hash = hash,
            PublishedAt = publishedAt,
            CrawledAt = article.PulledAt,
            UpdatedAt = article.PulledAt
        };

        try
        {
            await _fingerprints.InsertAsync(fingerprint, cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Another concurrent insert (a different provider's parallel crawl, or a manual
            // trigger overlapping a still-running scheduled one) won the race on the fingerprint's
            // Url/Hash unique index between ArticlePersister's own pre-check and this insert - the
            // same accepted narrow-race trade-off NewsArticleRepository.InsertAsync already
            // documents. Not a real failure, just a duplicate that lost the race.
            return;
        }

        // Shares the fingerprint's own client-generated Id 1:1, same reasoning as
        // NewsArticleRepository.InsertAsync - StringObjectIdGenerator already populated
        // fingerprint.Id above, so InsertOneAsync below sees a non-empty Id and won't generate one.
        article.Id = fingerprint.Id;
        await _collection.InsertOneAsync(article, options: null, cancellationToken);
    }

    public async Task<IReadOnlyList<FilteredArticle>> GetPagedAsync(int skip, int limit, CancellationToken cancellationToken) =>
        await _collection
            .Find(FilterDefinition<FilteredArticle>.Empty)
            .SortByDescending(a => a.PulledAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);

    public Task<long> CountAsync(CancellationToken cancellationToken) =>
        _collection.CountDocumentsAsync(FilterDefinition<FilteredArticle>.Empty, cancellationToken: cancellationToken);

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var result = await _collection.DeleteOneAsync(a => a.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<FilteredArticle>>
        {
            new(Builders<FilteredArticle>.IndexKeys.Descending(a => a.PulledAt),
                new CreateIndexOptions { Name = "ix_filteredarticle_pulledat" }),
            new(Builders<FilteredArticle>.IndexKeys.Ascending(a => a.Provider),
                new CreateIndexOptions { Name = "ix_filteredarticle_provider" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
