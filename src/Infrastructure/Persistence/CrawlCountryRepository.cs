using MongoDB.Bson;
using MongoDB.Driver;
using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class CrawlCountryRepository : ICrawlCountryRepository
{
    private readonly IMongoCollection<CrawlCountry> _collection;

    public CrawlCountryRepository(MongoDbContext context)
    {
        _collection = context.CrawlCountries;
    }

    public async Task<IReadOnlyList<CrawlCountry>> GetAllAsync(CrawlPipeline pipeline, CancellationToken cancellationToken) =>
        await _collection.Find(c => c.Pipeline == pipeline).ToListAsync(cancellationToken);

    public Task<CrawlCountry?> GetByNameAsync(CrawlPipeline pipeline, string name, CancellationToken cancellationToken) =>
        _collection.Find(c => c.Pipeline == pipeline && c.Name == name).FirstOrDefaultAsync(cancellationToken)!;

    public Task SeedIfMissingAsync(CrawlCountry country, CancellationToken cancellationToken)
    {
        var filter = BuildKeyFilter(country.Pipeline, country.Name);
        var update = Builders<CrawlCountry>.Update
            .SetOnInsert(c => c.Id, ObjectId.GenerateNewId().ToString())
            .SetOnInsert(c => c.Pipeline, country.Pipeline)
            .SetOnInsert(c => c.Name, country.Name)
            .SetOnInsert(c => c.Enabled, country.Enabled);

        return _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, cancellationToken);
    }

    public Task UpsertAsync(CrawlCountry country, CancellationToken cancellationToken)
    {
        var filter = BuildKeyFilter(country.Pipeline, country.Name);
        var update = Builders<CrawlCountry>.Update
            .SetOnInsert(c => c.Id, ObjectId.GenerateNewId().ToString())
            .Set(c => c.Enabled, country.Enabled);

        return _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<bool> DeleteAsync(CrawlPipeline pipeline, string name, CancellationToken cancellationToken)
    {
        var result = await _collection.DeleteOneAsync(BuildKeyFilter(pipeline, name), cancellationToken);
        return result.DeletedCount > 0;
    }

    private static FilterDefinition<CrawlCountry> BuildKeyFilter(CrawlPipeline pipeline, string name) =>
        Builders<CrawlCountry>.Filter.And(
            Builders<CrawlCountry>.Filter.Eq(c => c.Pipeline, pipeline),
            Builders<CrawlCountry>.Filter.Eq(c => c.Name, name));

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var model = new CreateIndexModel<CrawlCountry>(
            Builders<CrawlCountry>.IndexKeys.Ascending(c => c.Pipeline).Ascending(c => c.Name),
            new CreateIndexOptions { Name = "ux_crawlcountry_pipeline_name", Unique = true });

        await _collection.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);
    }
}
