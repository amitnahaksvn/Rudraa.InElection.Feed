using Domain.Entities;

namespace Application.ErrorLogs.Dtos;

/// <summary>Lightweight per-row projection of an <see cref="ErrorLog"/> for the error-monitor list - deliberately omits the large fields (stack trace, request/response bodies) that only <see cref="ErrorLogDetailDto"/> carries, so a page of 20 rows stays small.</summary>
public sealed record ErrorLogSummaryDto(
    string Id,
    DateTimeOffset CreatedOn,
    string ExceptionType,
    string Message,
    string Source,
    string? ErrorCode,
    string? Provider,
    string? FeedOrApiName,
    string? Country,
    int? HttpStatusCode,
    string Environment,
    string ApplicationName,
    bool IsResolved,
    DateTimeOffset? ResolvedOn)
{
    public static ErrorLogSummaryDto FromDomain(ErrorLog log) => new(
        log.Id,
        log.CreatedOn,
        log.ExceptionType,
        log.Message,
        log.Source,
        log.ErrorCode,
        log.Provider,
        log.FeedOrApiName,
        log.Country,
        log.HttpStatusCode,
        log.Environment,
        log.ApplicationName,
        log.IsResolved,
        log.ResolvedOn);
}
