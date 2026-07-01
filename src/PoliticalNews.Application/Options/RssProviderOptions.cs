namespace PoliticalNews.Application.Options;

/// <summary>
/// Configuration block for a single news provider (AajTak today; ANI/NDTV/PIB/etc. later).
/// Adding a new provider is purely additive: append a block here and register a matching
/// <see cref="Abstractions.IRssProvider"/> implementation whose Name matches <see cref="Name"/>.
/// </summary>
public sealed class RssProviderOptions
{
    /// <summary>Must match the <c>Name</c> exposed by the corresponding <see cref="Abstractions.IRssProvider"/>.</summary>
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public List<RssFeedOptions> Feeds { get; set; } = [];
}
