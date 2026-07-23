using FluentValidation;

namespace Application.Providers.Commands.TestApiEndpoint;

public sealed class TestApiEndpointCommandValidator : AbstractValidator<TestApiEndpointCommand>
{
    public TestApiEndpointCommandValidator()
    {
        RuleFor(c => c.EndpointId).NotEmpty();
    }
}
