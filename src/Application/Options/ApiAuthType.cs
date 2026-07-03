namespace Application.Options;

/// <summary>
/// How a news-API provider's key is attached to a request - kept configurable per provider since
/// the 7 providers under <see cref="NewsApiCrawlerOptions"/> split roughly evenly between the two
/// (e.g. NewsAPI.org/GNews/TheNewsAPI/Currents/Mediastack/NewsData.io use a query parameter;
/// WorldNewsAPI prefers an <c>x-api-key</c> header) rather than being a per-provider code branch.
/// </summary>
public enum ApiAuthType
{
    QueryParameter,
    HttpHeader,

    /// <summary>No API key at all (e.g. GDELT's public Doc API) - <c>BaseNewsApiProvider</c> skips the "NewsApiKeys" lookup entirely for these.</summary>
    None
}
