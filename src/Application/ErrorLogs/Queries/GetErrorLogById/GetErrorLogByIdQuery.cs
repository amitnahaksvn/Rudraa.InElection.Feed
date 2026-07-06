using Mediator;
using Application.Abstractions;
using Application.ErrorLogs.Dtos;

namespace Application.ErrorLogs.Queries.GetErrorLogById;

public sealed record GetErrorLogByIdQuery(string Id) : IRequest<ErrorLogDetailDto?>;

public sealed class GetErrorLogByIdQueryHandler : IRequestHandler<GetErrorLogByIdQuery, ErrorLogDetailDto?>
{
    private readonly IErrorLogRepository _errorLogs;

    public GetErrorLogByIdQueryHandler(IErrorLogRepository errorLogs)
    {
        _errorLogs = errorLogs;
    }

    public async ValueTask<ErrorLogDetailDto?> Handle(GetErrorLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _errorLogs.GetByIdAsync(request.Id, cancellationToken);
        return log is null ? null : ErrorLogDetailDto.FromDomain(log);
    }
}
