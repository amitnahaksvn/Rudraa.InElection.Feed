using MongoDB.Driver;
using Application.Abstractions;
using Application.Models;
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

    public async Task<IReadOnlyList<FilteredArticle>> GetPagedAsync(FilteredArticleFilter filter, int skip, int limit, CancellationToken cancellationToken) =>
        await _collection
            .Find(BuildFilter(filter))
            .SortByDescending(a => a.PulledAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);

    public Task<long> CountAsync(FilteredArticleFilter filter, CancellationToken cancellationToken) =>
        _collection.CountDocumentsAsync(BuildFilter(filter), cancellationToken: cancellationToken);

    private static FilterDefinition<FilteredArticle> BuildFilter(FilteredArticleFilter filter)
    {
        var builder = Builders<FilteredArticle>.Filter;
        var clauses = new List<FilterDefinition<FilteredArticle>>();

        if (!string.IsNullOrWhiteSpace(filter.Provider))
        {
            clauses.Add(builder.Eq(a => a.Provider, filter.Provider));
        }

        if (filter.SourceType is { } sourceType)
        {
            clauses.Add(builder.Eq(a => a.SourceType, sourceType));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            clauses.Add(builder.Eq(a => a.Category, filter.Category));
        }

        return clauses.Count == 0 ? FilterDefinition<FilteredArticle>.Empty : builder.And(clauses);
    }

    public async Task<IReadOnlyList<string>> GetDistinctProvidersAsync(CancellationToken cancellationToken)
    {
        var cursor = await _collection.DistinctAsync<string>("Provider", FilterDefinition<FilteredArticle>.Empty, cancellationToken: cancellationToken);
        var providers = await cursor.ToListAsync(cancellationToken);
        return providers.Where(p => !string.IsNullOrWhiteSpace(p)).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken)
    {
        var cursor = await _collection.DistinctAsync<string>("Category", FilterDefinition<FilteredArticle>.Empty, cancellationToken: cancellationToken);
        var categories = await cursor.ToListAsync(cancellationToken);
        return categories.Where(c => !string.IsNullOrWhiteSpace(c)).OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<long> DeleteManyAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var filter = Builders<FilteredArticle>.Filter.In(a => a.Id, ids);
        var result = await _collection.DeleteManyAsync(filter, cancellationToken);
        return result.DeletedCount;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<FilteredArticle>>
        {
            new(Builders<FilteredArticle>.IndexKeys.Descending(a => a.PulledAt),
                new CreateIndexOptions { Name = "ix_filteredarticle_pulledat" }),
            new(Builders<FilteredArticle>.IndexKeys.Ascending(a => a.Provider),
                new CreateIndexOptions { Name = "ix_filteredarticle_provider" }),
            new(Builders<FilteredArticle>.IndexKeys.Ascending(a => a.Category),
                new CreateIndexOptions { Name = "ix_filteredarticle_category" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
