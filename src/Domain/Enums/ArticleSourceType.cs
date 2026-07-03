namespace Domain.Enums;

/// <summary>
/// How an article was fetched - RSS/Atom feed (including the Mongo-driven <c>FeedSource</c>
/// pipeline and YouTube's channel feeds, both still RSS/Atom under the hood) vs a polled JSON
/// news API (NewsAPI.org, GDELT, ...). Recorded per article so callers can filter/report
/// by ingestion path without inferring it from <c>Provider</c> name.
/// </summary>
public enum ArticleSourceType
{
    Rss,
    Api
}
