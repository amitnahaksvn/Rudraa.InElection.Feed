using FluentValidation;

namespace Application.ErrorLogs.Queries.GetErrorLogById;

public sealed class GetErrorLogByIdQueryValidator : AbstractValidator<GetErrorLogByIdQuery>
{
    public GetErrorLogByIdQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty();
    }
}
