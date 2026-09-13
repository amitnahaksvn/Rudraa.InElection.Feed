using FluentValidation;

namespace Application.ErrorLogs.Commands.DeleteErrorLogs;

public sealed class DeleteErrorLogsCommandValidator : AbstractValidator<DeleteErrorLogsCommand>
{
    public DeleteErrorLogsCommandValidator()
    {
        RuleFor(c => c.Provider).NotEmpty();
    }
}
