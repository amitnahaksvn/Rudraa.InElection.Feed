namespace Application.Options;

/// <summary>
/// A single endpoint belonging to a <see cref="NewsApiProviderOptions"/> provider, as declared in
/// configuration - the <see cref="RssFeedOptions"/> counterpart for JSON news APIs. Most of these
/// providers expose more than one useful endpoint (e.g. NewsAPI.org's <c>/everything</c> search
/// vs <c>/top-headlines</c>; TheNewsAPI's <c>/all</c> vs <c>/top</c>), each with its own path and
/// query parameters but sharing the provider's base URL/auth/schedule - exactly like multiple RSS
/// feeds share one provider's <c>Cron</c>. Adding another endpoint for an already-wired provider
/// is purely a config change: append an entry here, no code change.
/// </summary>
public sealed class NewsApiEndpointOptions
{
    /// <summary>Human readable endpoint name, e.g. "Everything", "TopHeadlines".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Path appended to the owning provider's <see cref="NewsApiProviderOptions.BaseUrl"/>, e.g. "everything".</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Every query parameter this endpoint's request needs (topic/search term, language, country,
    /// category, page size, sort order, ...) besides the API key itself - see
    /// <see cref="NewsApiProviderOptions.AuthType"/>/<see cref="NewsApiProviderOptions.AuthParamName"/>.
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; set; } = [];

    /// <summary>Category assigned to every article pulled from this endpoint when the response itself doesn't carry one.</summary>
    public string Category { get; set; } = "General";

    public string Language { get; set; } = "en";

    public bool Enabled { get; set; } = true;
}
