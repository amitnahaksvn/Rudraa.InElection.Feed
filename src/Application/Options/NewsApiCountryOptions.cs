namespace Application.Options;

/// <summary>
/// A country-level grouping of JSON news-API providers, e.g. "India" or "United States" - the
/// <see cref="NewsApiCrawlerOptions"/> counterpart to <see cref="CountryOptions"/> for RSS. Lets
/// an entire country's worth of API providers be disabled with one flag (<see cref="Enabled"/>)
/// instead of flipping every provider under it individually, while each
/// <see cref="NewsApiProviderOptions.Enabled"/> and each endpoint's own
/// <see cref="NewsApiEndpointOptions.Enabled"/> still work exactly as before for narrower
/// control. A single-country institutional API (e.g. FEC, Congress.gov, UK Parliament Bills)
/// belongs under that one nation; a provider with no natural home nation (a global aggregator
/// configured to query a specific country via its own endpoint parameters, e.g. NewsAPI.org's
/// <c>country=in</c>, or a genuinely country-agnostic source like Google Fact Check) goes under
/// whichever country its current configuration actually targets, or "International" if none.
/// </summary>
public sealed class NewsApiCountryOptions
{
    /// <summary>Display name, e.g. "India", "United States" - also what shows up on <c>ErrorLog.Country</c> in a batched failure email, same as <see cref="CountryOptions.Name"/>.</summary>
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public List<NewsApiProviderOptions> Providers { get; set; } = [];
}
