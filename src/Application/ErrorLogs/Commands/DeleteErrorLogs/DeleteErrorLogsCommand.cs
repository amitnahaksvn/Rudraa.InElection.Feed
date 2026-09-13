using Mediator;
using Application.Abstractions;

namespace Application.ErrorLogs.Commands.DeleteErrorLogs;

/// <summary>
/// Bulk-deletes ErrorLogs rows once a provider's underlying failure cause has been fixed and
/// confirmed - a cleanup sweep, not something the error-monitor UI's per-row actions use.
/// <see cref="Provider"/> is required (validated non-empty) specifically so this can never run as
/// an unscoped delete-everything - every call must name the one provider it's cleaning up after.
/// <see cref="CreatedBefore"/> is optional but should normally be set to the fix's own deploy time,
/// so a genuinely new failure the fix didn't actually resolve is never silently deleted alongside
/// the old, already-diagnosed noise.
/// </summary>
public sealed record DeleteErrorLogsCommand(
    string Provider,
    DateTimeOffset? CreatedBefore = null) : IRequest<long>;

public sealed class DeleteErrorLogsCommandHandler : IRequestHandler<DeleteErrorLogsCommand, long>
{
    private readonly IErrorLogRepository _errorLogs;

    public DeleteErrorLogsCommandHandler(IErrorLogRepository errorLogs)
    {
        _errorLogs = errorLogs;
    }

    public ValueTask<long> Handle(DeleteErrorLogsCommand request, CancellationToken cancellationToken) =>
        new(_errorLogs.DeleteManyAsync(new ErrorLogFilter(Provider: request.Provider, CreatedBefore: request.CreatedBefore), cancellationToken));
}
