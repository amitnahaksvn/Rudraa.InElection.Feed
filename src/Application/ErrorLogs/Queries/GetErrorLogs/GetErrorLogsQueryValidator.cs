using FluentValidation;

namespace Application.ErrorLogs.Queries.GetErrorLogs;

public sealed class GetErrorLogsQueryValidator : AbstractValidator<GetErrorLogsQuery>
{
    public GetErrorLogsQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).GreaterThan(0).LessThanOrEqualTo(200);
    }
}
