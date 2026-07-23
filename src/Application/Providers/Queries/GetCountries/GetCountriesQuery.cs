using Mediator;
using Application.Abstractions;
using Application.Providers.Dtos;
using Domain.Enums;

namespace Application.Providers.Queries.GetCountries;

/// <summary>Every database-backed country for one pipeline - backs the Provider Management page's country list (add/toggle/delete).</summary>
public sealed record GetCountriesQuery(CrawlPipeline Pipeline) : IRequest<IReadOnlyList<CountryDto>>;

public sealed class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, IReadOnlyList<CountryDto>>
{
    private readonly ICrawlCountryRepository _countries;

    public GetCountriesQueryHandler(ICrawlCountryRepository countries)
    {
        _countries = countries;
    }

    public async ValueTask<IReadOnlyList<CountryDto>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await _countries.GetAllAsync(request.Pipeline, cancellationToken);
        return countries.Select(CountryDto.FromDomain).ToList();
    }
}
