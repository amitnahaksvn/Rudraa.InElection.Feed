using Domain.Entities;

namespace Application.Abstractions;

/// <summary>Persistence for <see cref="ErrorLog"/> - the general app-wide exception log.</summary>
public interface IErrorLogRepository
{
    Task InsertAsync(ErrorLog errorLog, CancellationToken cancellationToken);

    /// <summary>Oldest-first, up to <paramref name="limit"/> rows with <see cref="ErrorLog.IsSent"/> still false.</summary>
    Task<IReadOnlyList<ErrorLog>> GetUnsentAsync(int limit, CancellationToken cancellationToken);

    /// <summary>Flips <see cref="ErrorLog.IsSent"/> to true and stamps <see cref="ErrorLog.SentOn"/> for exactly these ids - called only after a dispatch email has actually been sent successfully.</summary>
    Task MarkAsSentAsync(IReadOnlyList<string> ids, DateTimeOffset sentOn, CancellationToken cancellationToken);

    Task EnsureIndexesAsync(CancellationToken cancellationToken);
}
