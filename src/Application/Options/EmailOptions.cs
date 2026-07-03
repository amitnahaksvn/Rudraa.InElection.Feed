namespace Application.Options;

/// <summary>
/// Root configuration section ("Email") controlling the monitoring-alert email service. Lives in
/// <c>NewsCrawler.appsettings.json</c> (shared by Web and Worker, same reasoning as every other
/// section there) since both hosts run the same crawl orchestrators that raise these alerts.
/// Provider-agnostic by design - nothing here is Resend-specific - so swapping the concrete
/// <see cref="Abstractions.IEmailService"/> implementation for SendGrid/SES/SMTP/etc. later never
/// requires touching this class or any caller.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Master switch - when false, every <see cref="Abstractions.IEmailService"/> method is a logged no-op.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Provider API key/token. Set via <c>Email__ApiKey</c> env var or user-secrets in production, never committed for real - see CLAUDE.md.</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public List<string> To { get; set; } = [];

    /// <summary>Retry attempts for transient failures when calling the underlying email provider's API.</summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
