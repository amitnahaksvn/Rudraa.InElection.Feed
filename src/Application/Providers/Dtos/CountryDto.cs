using Domain.Entities;

namespace Application.Providers.Dtos;

public sealed record CountryDto(string Id, string Pipeline, string Name, bool Enabled)
{
    public static CountryDto FromDomain(CrawlCountry country) => new(country.Id, country.Pipeline.ToString(), country.Name, country.Enabled);
}
