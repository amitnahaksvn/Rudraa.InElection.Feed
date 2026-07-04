namespace Web.Options;

/// <summary>Root configuration section ("Api") controlling read-endpoint defaults and limits.</summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public int DefaultPageSize { get; set; } = 20;

    public int MaxPageSize { get; set; } = 100;

    public bool EnableSwagger { get; set; } = true;

    /// <summary>
    /// Hangfire's dashboard has no built-in auth and none is applied here - anyone who reaches
    /// this URL can view job internals and trigger/delete jobs, not just view them. Off by
    /// default; enabling this on a public deployment is a deliberate convenience-over-security
    /// trade-off, made knowingly, not a default to flip on casually.
    /// </summary>
    public bool EnableHangfireDashboard { get; set; }
}
