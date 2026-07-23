using Mediator;
using Application.Abstractions;
using Application.Providers.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Providers.Commands.UpsertCountry;

/// <summary>Creates a brand-new country or updates an existing one's Enabled flag - the "add country"/enable-toggle path from the Provider Management page.</summary>
public sealed record UpsertCountryCommand(CrawlPipeline Pipeline, string Name, bool Enabled) : IRequest<CountryDto>;

public sealed class UpsertCountryCommandHandler : IRequestHandler<UpsertCountryCommand, CountryDto>
{
    private readonly ICrawlCountryRepository _countries;

    public UpsertCountryCommandHandler(ICrawlCountryRepository countries)
    {
        _countries = countries;
    }

    public async ValueTask<CountryDto> Handle(UpsertCountryCommand request, CancellationToken cancellationToken)
    {
        var country = new CrawlCountry { Pipeline = request.Pipeline, Name = request.Name, Enabled = request.Enabled };
        await _countries.UpsertAsync(country, cancellationToken);
        return new CountryDto(country.Id, request.Pipeline.ToString(), request.Name, request.Enabled);
    }
}
