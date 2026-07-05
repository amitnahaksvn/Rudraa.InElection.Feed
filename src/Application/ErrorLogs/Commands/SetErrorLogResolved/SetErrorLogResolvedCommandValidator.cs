using FluentValidation;

namespace Application.ErrorLogs.Commands.SetErrorLogResolved;

public sealed class SetErrorLogResolvedCommandValidator : AbstractValidator<SetErrorLogResolvedCommand>
{
    public SetErrorLogResolvedCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
