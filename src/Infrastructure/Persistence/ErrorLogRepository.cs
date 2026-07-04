using MongoDB.Driver;
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Mongo;

namespace Infrastructure.Persistence;

public sealed class ErrorLogRepository : IErrorLogRepository
{
    private readonly IMongoCollection<ErrorLog> _collection;

    public ErrorLogRepository(MongoDbContext context)
    {
        _collection = context.ErrorLogs;
    }

    public Task InsertAsync(ErrorLog errorLog, CancellationToken cancellationToken) =>
        _collection.InsertOneAsync(errorLog, options: null, cancellationToken);

    public async Task<IReadOnlyList<ErrorLog>> GetUnsentAsync(int limit, CancellationToken cancellationToken) =>
        await _collection
            .Find(e => !e.IsSent)
            .SortBy(e => e.CreatedOn)
            .Limit(limit)
            .ToListAsync(cancellationToken);

    public async Task MarkAsSentAsync(IReadOnlyList<string> ids, DateTimeOffset sentOn, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return;
        }

        await _collection.UpdateManyAsync(
            Builders<ErrorLog>.Filter.In(e => e.Id, ids),
            Builders<ErrorLog>.Update.Set(e => e.IsSent, true).Set(e => e.SentOn, sentOn),
            cancellationToken: cancellationToken);
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var models = new List<CreateIndexModel<ErrorLog>>
        {
            // Covers ErrorNotificationDispatchService's GetUnsentAsync query (filter IsSent=false,
            // sort by CreatedOn) directly from the index, without an in-memory sort.
            new(Builders<ErrorLog>.IndexKeys.Ascending(e => e.IsSent).Ascending(e => e.CreatedOn),
                new CreateIndexOptions { Name = "ix_errorlog_issent_createdon" }),
            new(Builders<ErrorLog>.IndexKeys.Descending(e => e.CreatedOn),
                new CreateIndexOptions { Name = "ix_errorlog_createdon" }),
            new(Builders<ErrorLog>.IndexKeys.Ascending(e => e.Provider),
                new CreateIndexOptions { Name = "ix_errorlog_provider" })
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken);
    }
}
