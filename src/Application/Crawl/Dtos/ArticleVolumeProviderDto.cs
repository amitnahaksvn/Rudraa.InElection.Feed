namespace Application.Crawl.Dtos;

/// <summary>One provider's row in the article-volume report's breakdown table - a real count of ArticleFingerprint documents within the selected date range. Zero for a configured/selected provider that simply had no articles ingested in the window, so the table is always the complete provider list rather than only the ones with activity.</summary>
public sealed record ArticleVolumeProviderDto(string Provider, int Count);
