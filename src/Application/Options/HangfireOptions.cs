namespace Application.Options;

/// <summary>
/// Root configuration section ("Hangfire") controlling how <c>Web</c>'s Hangfire server itself is
/// tuned - which queues it pulls jobs from and how many it processes concurrently. Web is the only
/// host that calls <c>AddHangfireServer()</c> - it owns both HTTP serving and job execution.
///
/// This exists so a future deployment could split job processing into separate replica groups of
/// the same image, each scaled independently, by giving each group a different
/// <see cref="Queues"/> value via environment variable (e.g. one group with
/// <c>Hangfire__Queues__0=rss</c>, another with <c>Hangfire__Queues__0=api</c>) - a lever kept
/// available even though the current deployment runs everything as one free-tier-friendly process.
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
    /// <c>HangfireSocialMediaJobExecutor</c>; "default" is included so untagged jobs (e.g. the
    /// raw-response cleanup and error-notification-dispatch jobs) still run.
    ///
    /// "keepalive" is listed first, ahead of every content-crawling queue, and is exactly one
    /// job: <c>HangfireKeepAliveExecutor</c>'s self-ping, tagged <c>[Queue("keepalive")]</c>. This
    /// isn't just tidiness - on this app's single free-tier instance with a small WorkerCount, a
    /// burst of 100+ freshly-due rss/api crawl jobs (the normal shape right after any wake-up, when
    /// many providers' crons have all come due at once) can occupy every worker thread for minutes;
    /// if keep-alive shared "default" behind them in priority, it could be starved long enough to
    /// miss its own once-a-minute deadline, letting Render's free-tier host spin back down despite
    /// the self-ping technically running successfully every time it does get a turn - confirmed
    /// happening in production before this queue was split out (successful keep-alive-ping runs
    /// arriving in tight bursts followed by multi-minute gaps, then stopping altogether once a
    /// large rss backlog piled up). Giving it its own always-drained-first queue means it can never
    /// be delayed by however much crawl volume is queued behind it, regardless of WorkerCount.
    ///
    /// All five are listed by default so a single instance processes everything out of the box; a
    /// deployment that wants these as independently scaled replica groups would set this to just
    /// <c>["rss"]</c> on one group, <c>["api"]</c> on another, <c>["social"]</c> on a third - each
    /// still needs "keepalive" too if it's the one expected to keep the free-tier host awake.
    /// </summary>
    public string[] Queues { get; set; } = ["keepalive", "rss", "api", "social", "default"];

    /// <summary>
    /// Concurrent jobs this server instance processes. Null keeps Hangfire's own default
    /// (<c>Environment.ProcessorCount * 5</c>).
    /// </summary>
    public int? WorkerCount { get; set; }
}
