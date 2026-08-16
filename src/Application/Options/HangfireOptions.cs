namespace Application.Options;

/// <summary>
/// Root configuration section ("Hangfire") controlling how a host's own Hangfire server is tuned -
/// which queues it pulls jobs from and how many it processes concurrently. WebRssFeed and
/// WebApiFeed each bind their own instance of this and each supplies its own fallback default for
/// <see cref="Queues"/> in its own <c>Program.cs</c> if unconfigured (WebRssFeed: rss/default;
/// WebApiFeed: api/social) - <em>not</em> a shared default on this class, since a shared
/// "process everything" default would be actively wrong for either host once split (see
/// <see cref="Queues"/>'s own doc comment for why it can't default to a non-empty array here at
/// all).
/// </summary>
public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    /// <summary>
    /// Queue names this server instance pulls jobs from, in priority order - Hangfire always
    /// drains an earlier-listed queue before touching a later one, on every fetch cycle. Every
    /// recurring RSS crawl job is tagged <c>[Queue("rss")]</c> on <c>HangfireCrawlJobExecutor</c>,
    /// every JSON news-API fetch job <c>[Queue("api")]</c> on <c>HangfireNewsApiJobExecutor</c>,
    /// every Social pipeline poll job <c>[Queue("social")]</c> on
    /// <c>HangfireSocialMediaJobExecutor</c>; "default" is for untagged jobs (e.g. the
    /// raw-response cleanup and error-notification-dispatch jobs, WebRssFeed's own).
    ///
    /// Deliberately defaults to an empty array here, <em>not</em> a hardcoded queue list: .NET's
    /// <see cref="Microsoft.Extensions.Configuration.ConfigurationBinder"/> APPENDS a config
    /// section's array items after whatever a target array property is already initialized to,
    /// rather than replacing it - confirmed directly (a `new HangfireOptions()` default of
    /// `["rss","api","social","default"]` bound against WebApiFeed's own configured
    /// `["api","social"]` resolved to all entries concatenated, silently re-adding "rss" to a host
    /// that must never process it). Each host's own `Program.cs` supplies its own safe fallback
    /// explicitly, after binding, instead of relying on a property initializer here.
    /// </summary>
    public string[] Queues { get; set; } = [];

    /// <summary>
    /// Concurrent jobs this server instance processes. Null keeps Hangfire's own default
    /// (<c>Environment.ProcessorCount * 5</c>).
    /// </summary>
    public int? WorkerCount { get; set; }

    /// <summary>
    /// Real MongoDB (not Cosmos DB) connection string backing Hangfire's own job storage -
    /// deliberately kept separate from <c>CosmosDbOptions.ConnectionString</c>, which every other
    /// collection in this app (NewsArticles, CrawlHistory, ...) uses. Hangfire.Mongo needs capped
    /// collections and a backup-then-migrate schema strategy that Azure Cosmos DB's Mongo API
    /// (RU-based) doesn't support - confirmed live against this app's real Cosmos DB account
    /// (capped-collection creation and migration-backup both failed outright) - so Hangfire keeps
    /// running against a real MongoDB instance instead, while everything else moved to Cosmos.
    /// </summary>
    public string MongoConnectionString { get; set; } = "mongodb://localhost:27017";

    /// <summary>Database name on <see cref="MongoConnectionString"/>'s server - see its own doc comment.</summary>
    public string MongoDatabaseName { get; set; } = "PoliticalNewsDb";
}
