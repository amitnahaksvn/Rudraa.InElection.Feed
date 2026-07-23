using MongoDB.Driver;
using Application.Abstractions;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class NewsArticleRepository : INewsArticleRepository
{
    private readonly IMongoCollection<NewsArticle> _collection;
    private readonly IArticleFingerprintRepository _fingerprints;

    public NewsArticleRepository(MongoDbContext context, IArticleFingerprintRepository fingerprints)
    {
        _collection = context.NewsArticles;
        _fingerprints = fingerprints;
    }

    /// <summary>
    /// Every dedup check below is resolved against <see cref="IArticleFingerprintRepository"/> -
    /// a lean per-article record (Url/OriginalGuid/Hash/CrawledAt) - so a duplicate skip (the
    /// overwhelming majority of crawls, since most ticks just re-see articles already stored)
    /// never loads the full NewsArticle document (Title/Summary/Content/ImageUrl/Tags/Metadata,
    /// the bulk of a document's actual size). An existing article is never modified in place -
    /// any match on Url, OriginalGuid, or Hash is always a duplicate skip, regardless of whether
    /// the incoming content differs; only a genuinely new article gets inserted.
    /// </summary>
    public async Task<ArticleUpsertOutcome> UpsertAsync(NewsArticle article, CancellationToken cancellationToken)
    {
        var existing = await _fingerprints.FindDuplicateAsync(article.Url, article.OriginalGuid, article.Hash, cancellationToken);
        return existing is not null
            ? ArticleUpsertOutcome.DuplicateSkipped
            : await InsertAsync(article, cancellationToken);
    }

    /// <summary>
    /// Reserves the fingerprint first (cheap - just a handful of strings, gated by its own unique
    /// Url/Hash indexes) and only inserts the full article once that succeeds, so two providers
    /// racing on the same new story never both write a full duplicate document - the fingerprint
    /// insert alone decides the race. The narrow trade-off: a crash between the two inserts leaves
    /// an orphaned fingerprint with no article, which would make every future crawl of that same
    /// Url/story silently skip it forever. Accepted as-is (same category as this codebase's other
    /// documented narrow-race trade-offs) rather than adding a multi-document transaction for a
    /// window this small.
    /// </summary>
    private async Task<ArticleUpsertOutcome> InsertAsync(NewsArticle article, CancellationToken cancellationToken)
    {
        var fingerprint = new ArticleFingerprint
        {
            Provider = article.Provider,
            SourceType = article.SourceType,
            Url = article.Url,
            OriginalGuid = article.OriginalGuid,
            Hash = article.Hash,
            PublishedAt = article.PublishedAt,
            CrawledAt = article.CrawledAt,
            UpdatedAt = article.UpdatedAt
        };

        try
        {
            await _fingerprints.InsertAsync(fingerprint, cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Another concurrent insert (a different provider's parallel crawl, or a manual
            // trigger overlapping a still-running scheduled one) won the race on the fingerprint's
            // Url/Hash unique index between this method's find-checks and this InsertAsync - the
            // article is already persisted (or about to be), so this is a duplicate, not a real
            // failure. Without this, an uncaught exception here would abort the *entire* crawl run
            // as Failed over what is actually a successful dedup.
            return ArticleUpsertOutcome.DuplicateSkipped;
        }

        // Shares the fingerprint's own client-generated Id 1:1 - StringObjectIdGenerator already
        // populated fingerprint.Id above, so InsertOneAsync below sees a non-empty Id and won't
        // generate a second one.
        article.Id = fingerprint.Id;
        await _collection.InsertOneAsync(article, options: null, cancellationToken);
        return ArticleUpsertOutcome.Inserted;
    }

    public async Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int count, CancellationToken cancellationToken) =>
        await _collection.Find(a => a.IsActive)
            .SortByDescending(a => a.CrawledAt)
            .Limit(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NewsArticle>> GetByProviderAsync(string provider, int count, CancellationToken cancellationToken) =>
        await _collection.Find(a => a.IsActive && a.Provider == provider)
            .SortByDescending(a => a.CrawledAt)
            .Limit(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NewsArticle>> GetByCategoryAsync(string category, int count, CancellationToken cancellationToken) =>
        await _collection.Find(a => a.IsActive && a.Category == category)
            .SortByDescending(a => a.CrawledAt)
            .Limit(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NewsArticle>> SearchAsync(string query, int count, CancellationToken cancellationToken)
    {
        var filter = Builders<NewsArticle>.Filter.And(
            Builders<NewsArticle>.Filter.Eq(a => a.IsActive, true),
            Builders<NewsArticle>.Filter.Or(
                Builders<NewsArticle>.Filter.Regex(a => a.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<NewsArticle>.Filter.Regex(a => a.Summary, new MongoDB.Bson.BsonRegularExpression(query, "i"))));

        return await _collection.Find(filter)
            .SortByDescending(a => a.CrawledAt)
            .Limit(count)
            .ToListAsync(cancellationToken);
    }

    // Deliberately reuses the existing ix_news_active_crawledat index (IsActive + CrawledAt desc)
    // rather than adding new SourceType/Country-specific compound indexes the way
    // GetByProviderAsync/GetByCategoryAsync's own indexes do - this database is currently at its
    // Atlas free-tier storage cap with writes blocked, so adding index storage right now would
    // make that worse, not better. Mongo still uses ix_news_active_crawledat for the IsActive
    // equality + CrawledAt sort here and filters SourceType/Country per-document from there; add
    // dedicated compound indexes once there's headroom again if this needs to scale further.
    //
    // Defaults to sorting by PublishedAt (falling back to CrawledAt, both descending) rather than
    // CrawledAt alone - the News Feed page's own card (ArticleCard.tsx) displays
    // `article.publishedAt ?? article.crawledAt`, so sorting purely by CrawledAt made the
    // *displayed* timestamps jump around non-monotonically (a feed mixing providers with very
    // different publish-to-crawl lag looks "out of order" even though the crawl-time order was
    // technically consistent). filter.SortBy also lets the page switch to pure CrawledAt order,
    // and filter.SortDirection lets it flip to oldest-first, for readers who want either. None of
    // that costs a new index - ix_news_publishedat (a plain descending index) already existed
    // unused, and ix_news_active_crawledat already backed the old CrawledAt default; Mongo can
    // still use ix_news_publishedat to satisfy an *ascending* PublishedAt sort too (a single-field
    // index supports both directions). The CrawledAt tiebreaker on the PublishedAt branch (same
    // direction as the primary sort) breaks ties for the minority of articles with no PublishedAt
    // at all - sorted to whichever end that direction puts nulls - and for same-instant
    // PublishedAt values.
    public async Task<IReadOnlyList<NewsArticle>> GetFeedAsync(NewsArticleFeedFilter filter, CancellationToken cancellationToken)
    {
        var query = _collection.Find(BuildFeedFilter(filter));
        var ascending = filter.SortDirection == NewsFeedSortDirection.Ascending;

        var sorted = filter.SortBy == NewsFeedSortBy.CrawledAt
            ? (ascending ? query.SortBy(a => a.CrawledAt) : query.SortByDescending(a => a.CrawledAt))
            : (ascending
                ? query.SortBy(a => a.PublishedAt).ThenBy(a => a.CrawledAt)
                : query.SortByDescending(a => a.PublishedAt).ThenByDescending(a => a.CrawledAt));

        return await sorted.Skip(filter.Skip).Limit(filter.Take).ToListAsync(cancellationToken);
    }

    /// <summary>Total articles matching the same pipeline/country narrowing as <see cref="GetFeedAsync"/> (its Skip/Take are irrelevant here) - backs the News Feed page's total-count header.</summary>
    public Task<long> CountFeedAsync(NewsArticleFeedFilter filter, CancellationToken cancellationToken) =>
        _collection.CountDocumentsAsync(BuildFeedFilter(filter), cancellationToken: cancellationToken);

    private static FilterDefinition<NewsArticle> BuildFeedFilter(NewsArticleFeedFilter filter)
    {
        var builder = Builders<NewsArticle>.Filter;
        var clauses = new List<FilterDefinition<NewsArticle>> { builder.Eq(a => a.IsActive, true) };

        if (filter.SourceType is { } sourceType)
        {
            clauses.Add(builder.Eq(a => a.SourceType, sourceType));
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            clauses.Add(builder.Eq(a => a.Country, filter.Country));
        }

        return builder.And(clauses);
    }

    public async Task<IReadOnlyList<string>> GetDistinctCountriesAsync(ArticleSourceType? sourceType, CancellationToken cancellationToken)
    {
        var builder = Builders<NewsArticle>.Filter;
        var filter = sourceType is { } st
            ? builder.And(builder.Eq(a => a.IsActive, true), builder.Eq(a => a.SourceType, st))
            : builder.Eq(a => a.IsActive, true);

        var cursor = await _collection.DistinctAsync<string>("Country", filter, cancellationToken: cancellationToken);
        var countries = await cursor.ToListAsync(cancellationToken);

        return countries
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<long> DeleteManyAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var filter = Builders<NewsArticle>.Filter.In(a => a.Id, ids);
        var update = Builders<NewsArticle>.Update.Set(a => a.IsActive, false);
        var result = await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<NewsArticle>>
        {
            new(Builders<NewsArticle>.IndexKeys.Descending(a => a.PublishedAt),
                new CreateIndexOptions { Name = "ix_news_publishedat" }),
            // Compound, not single-field, because every actual read query
            // (GetLatestAsync/GetByProviderAsync/GetByCategoryAsync) filters on IsActive (plus
            // Provider/Category for the latter two) and always sorts by CrawledAt descending -
            // Mongo can only use one index per query, so three separate single-field indexes here
            // forced an index scan plus an in-memory sort instead of one fully-covered compound
            // index. No query filters on Provider/Category/CrawledAt without IsActive, so these
            // fully subsume what the old single-field indexes covered.
            new(Builders<NewsArticle>.IndexKeys.Ascending(a => a.IsActive).Descending(a => a.CrawledAt),
                new CreateIndexOptions { Name = "ix_news_active_crawledat" }),
            new(Builders<NewsArticle>.IndexKeys.Ascending(a => a.IsActive).Ascending(a => a.Provider).Descending(a => a.CrawledAt),
                new CreateIndexOptions { Name = "ix_news_active_provider_crawledat" }),
            new(Builders<NewsArticle>.IndexKeys.Ascending(a => a.IsActive).Ascending(a => a.Category).Descending(a => a.CrawledAt),
                new CreateIndexOptions { Name = "ix_news_active_category_crawledat" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);

        await DropSupersededSingleFieldIndexesAsync(cancellationToken);
    }

    // ux_news_url/ux_news_hash/ix_news_hash/ix_news_originalguid are superseded by
    // ArticleFingerprintRepository's own indexes now that dedup no longer queries NewsArticles
    // directly - dropped here (rather than left behind) purely to reclaim their storage, the same
    // reasoning that already applied to ix_news_crawledat/ix_news_provider/ix_news_category below.
    private static readonly string[] SupersededIndexNames =
    [
        "ix_news_crawledat", "ix_news_provider", "ix_news_category",
        "ux_news_url", "ux_news_hash", "ix_news_hash", "ix_news_originalguid"
    ];

    /// <summary>
    /// Drops indexes superseded by a later change (see <see cref="SupersededIndexNames"/>'s own
    /// comment for what replaced each one) - an existing database would otherwise carry both the
    /// old and new indexes forever, paying the write/storage overhead of indexes nothing queries
    /// anymore.
    /// </summary>
    private async Task DropSupersededSingleFieldIndexesAsync(CancellationToken cancellationToken)
    {
        var existingIndexes = await (await _collection.Indexes.ListAsync(cancellationToken)).ToListAsync(cancellationToken);
        var existingNames = existingIndexes.Select(i => i["name"].AsString).ToHashSet(StringComparer.Ordinal);

        foreach (var indexName in SupersededIndexNames)
        {
            if (existingNames.Contains(indexName))
            {
                await _collection.Indexes.DropOneAsync(indexName, cancellationToken);
            }
        }
    }
}
