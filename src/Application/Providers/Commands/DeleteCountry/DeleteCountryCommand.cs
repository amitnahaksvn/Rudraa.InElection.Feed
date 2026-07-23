using Mediator;
using Application.Abstractions;
using Domain.Enums;

namespace Application.Providers.Commands.DeleteCountry;

/// <summary>Deletes a country - the providers/feeds that referenced it are left alone (they just point at a country name that no longer exists, the same way an orphaned foreign key would), since deleting a country is not expected to cascade-delete every provider under it.</summary>
public sealed record DeleteCountryCommand(CrawlPipeline Pipeline, string Name) : IRequest<bool>;

public sealed class DeleteCountryCommandHandler : IRequestHandler<DeleteCountryCommand, bool>
{
    private readonly ICrawlCountryRepository _countries;

    public DeleteCountryCommandHandler(ICrawlCountryRepository countries)
    {
        _countries = countries;
    }

    public async ValueTask<bool> Handle(DeleteCountryCommand request, CancellationToken cancellationToken) =>
        await _countries.DeleteAsync(request.Pipeline, request.Name, cancellationToken);
}
