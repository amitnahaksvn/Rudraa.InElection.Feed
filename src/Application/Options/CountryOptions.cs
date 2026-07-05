namespace Application.Options;

/// <summary>
/// A country-level grouping of RSS providers, e.g. "India" or "United Kingdom" - lets an entire
/// country's worth of feeds be disabled with one flag (<see cref="Enabled"/>) instead of having to
/// flip every provider under it individually, while each <see cref="RssProviderOptions.Enabled"/>
/// and each feed's own <see cref="RssFeedOptions.Enabled"/> still work exactly as before for
/// narrower control. Three independent levels of on/off: country, provider, feed.
/// </summary>
public sealed class CountryOptions
{
    /// <summary>Display name, e.g. "India", "United States" - also what shows up on <c>ErrorLog.Country</c> in a batched failure email.</summary>
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public List<RssProviderOptions> Providers { get; set; } = [];
}
